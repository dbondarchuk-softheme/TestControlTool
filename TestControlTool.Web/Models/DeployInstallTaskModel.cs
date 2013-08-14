using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;

namespace TestControlTool.Web.Models
{
    public class DeployInstallTaskModel
    {
        public string Name { get; set; }

        public DeployInstallType Type { get; set; }

        public IEnumerable<Guid> Machines { get; set; }

        public string Version { get; set; }

        public string Build { get; set; }

        public static DeployInstallTaskModel GetFromXmlFile(string file)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var split = file.Split('.');
            var name = split.ElementAt(split.Length - 2);

            var container = Core.Extensions.DeserializeFromFile<DeployInstallTaskContainer>(file);

            return new DeployInstallTaskModel
                {
                    Machines = container.Machines,
                    Name = name,
                    Type = container.DeployInstallType,
                    Version = container.BuildVersion,
                    Build = container.BuildNumber
                };
        }

        public void SaveToFile(string file, string user)
        {
            var fileToSave = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var container = new DeployInstallTaskContainer
                {
                    Machines = Machines.ToList(),
                    Files = new List<Pair<VMServerType, string>>(),
                    BuildVersion = Version,
                    BuildNumber = Build,
                    DeployInstallType = Type
                };

            var account = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == user);

            var vmServers = account.VMServers.Where(x => account.Machines.Any(y => y.Server == x.Id && Machines.Contains(y.Id)));

            var serverMap = vmServers.ToDictionary(x => x,
                y => new Pair<string, IEnumerable<IMachine>>(
                    file.Remove(file.LastIndexOf('.')) + '.' + y.Id + ".xml",
                    TestControlToolApplication.AccountController.CachedMachines.Where(z => z.Server == y.Id && Machines.Contains(z.Id))));

            foreach (var item in serverMap)
            {
                container.Files.Add(new Pair<VMServerType, string>(item.Key.Type, item.Value.Key));
            }

            foreach (var item in serverMap)
            {
                if (item.Key.Type == VMServerType.VCenter)
                {
                    SaveVCenterDeployInstallModel(item);
                }
                else if (item.Key.Type == VMServerType.HyperV)
                {
                    SaveHyperVDeployInstallModel(item);
                }
            }

            container.SerializeToFile(fileToSave);
        }

        private void SaveVCenterDeployInstallModel(KeyValuePair<VMServer, Pair<string, IEnumerable<IMachine>>> item)
        {
            var sourceFile = File.ReadAllText(ConfigurationManager.AppSettings["TasksFolder"] + "\\VCenterAutodeploySource.xml");

            sourceFile = sourceFile.Replace("{$BUILD_VERSION}", Version).Replace("{$BUILD_NUMBER}", Build).Replace("{$ACTION_TYPE}", Type.ToString())
                .Replace("SERVER_ID", item.Key.Id.ToString());

            var machineLine = sourceFile.Remove(sourceFile.IndexOf("</VM>", StringComparison.Ordinal))
                .Substring(sourceFile.IndexOf("<VM ReplayType", StringComparison.Ordinal)) + "</VM>";

            if (machineLine == null) throw new InvalidOperationException("Wrong XML file for autodeploy");

            var machinesLines = new string[item.Value.Value.Count()];

            for (var i = 0; i < item.Value.Value.Count(); i++)
            {
                machinesLines[i] = machineLine.Replace("MACHINE_ID", item.Value.Value.ElementAt(i).Id.ToString());
            }

            sourceFile = sourceFile.Replace(machineLine, machinesLines.Aggregate("", (init, s) => init + "\n" + s));

            File.WriteAllText(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + item.Value.Key, sourceFile, new UnicodeEncoding());

        }

        private void SaveHyperVDeployInstallModel(KeyValuePair<VMServer, Pair<string, IEnumerable<IMachine>>> item)
        {
            var sourceFile = File.ReadAllLines(ConfigurationManager.AppSettings["TasksFolder"] + "\\HyperVAutodeploySource.xml");

            for (var i = 0; i < sourceFile.Length; i++)
            {
                sourceFile[i] = sourceFile[i].Replace("{$BUILD_VERSION}", Version).Replace("{$BUILD_NUMBER}", Build).Replace("{$ACTION_TYPE}", Type.ToString())
                    .Replace("SERVER_ID", item.Key.Id.ToString());
            }

            var machineLine = sourceFile.FirstOrDefault(x => x.Trim().StartsWith("<machine"));

            if (machineLine == null) throw new InvalidOperationException("Wrong XML file for autodeploy");

            var machinesLines = new string[item.Value.Value.Count()];

            for (var i = 0; i < item.Value.Value.Count(); i++)
            {
                machinesLines[i] = machineLine.Replace("MACHINE_ID", item.Value.Value.ElementAt(i).Id.ToString());
            }

            sourceFile[sourceFile.ToList().IndexOf(machineLine)] = machinesLines.Aggregate("", (init, s) => init + "\n" + s);

            File.WriteAllLines(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + item.Value.Key, sourceFile, new UnicodeEncoding());
        }
    }
}