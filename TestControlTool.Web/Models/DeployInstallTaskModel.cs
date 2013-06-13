using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using AutodeploymentConsole.Models;
using AutodeploymentServices.Models;
using AutodeploymentServices.Models.InstallationModel;
using AutodeploymentServices.Models.VCenterDeployModel;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Web.Models
{
    public enum AutodeployJobType
    {
        Deploy,
        Install,
        DeployInstall
    }

    public class DeployInstallTaskModel
    {
        public string Name { get; set; }

        public AutodeployJobType Type { get; set; }

        public IEnumerable<Guid> Machines { get; set; }

        public string Version { get; set; }

        public static DeployInstallTaskModel GetFromXmlFile(string file)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var split = file.Split('.');
            var name = split.ElementAt(split.Length - 2);

            var set = AutodeploymentModel.LoadFromXmlFile(file).GetVirtualMachinesSet("1");

            var version = set.DefaultBuild.ReplayVersion;

            var machines = set.VirtualMachines.Select(x => new Guid(x.DeployInfo.VirtualMachine.VMName.Substring(2, 36)));

            var type = (AutodeployJobType)Enum.Parse(typeof(TestControlTool.Web.Models.AutodeployJobType), set.JobType.ToString(), true);

            return new DeployInstallTaskModel
                {
                    Machines = machines,
                    Name = name,
                    Type = type,
                    Version = version
                };
        }

        public void SaveToFile(string file, string user)
        {
            file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + file;

            var model = AutodeploymentModel.LoadFromXmlFile(ConfigurationManager.AppSettings["TasksFolder"] + "\\AutodeploySource.xml");

            var machines = new Collection<AutodeployMachineInfo>(Machines.Select(machine => new AutodeployMachineInfo
                {
                    InstallationInfo = new InstallationMachineInfo
                        {
                            MachineCredentials = new VirtualMachineCredentials("{$" + machine + "/Address}", "{$" + machine + "/UserName}", "{$" + machine + "/Password}"),
                            Share = "{$" + machine + "/Share}"
                        }, DeployInfo = new DeployMachineInfo
                            {
                                TemplateVirtualMachine = new VCenterVMInfo("{$" + machine + "/TemplateVMName}", "{$" + machine + "/TemplateInventoryPath}"),
                                VirtualMachine = new VCenterVMInfo("{$" + machine + "/VirtualMachineVMName}", "{$" + machine + "/VirtualMachineInventoryPath}", "{$" + machine + "/VirtualMachineResourcePool}")
                            }, ReplayType = ReplayVMType.Core
                }).ToList());

            model.VCenterLoginAlias = user;
            model.VirtualMachinesSets.Add(new AutodeployMachineSet
                {
                    DefaultBuild = new BuildInfo
                        {
                            ReplayVersion = Version
                        },
                        Id = "1",
                    JobType = (AutodeploymentConsole.Models.AutodeployJobType)Enum.Parse(typeof(
                    AutodeploymentConsole.Models.AutodeployJobType), Type.ToString(), true),
                    VirtualMachines = machines
                });

            model.SaveToXmlFile(file);
        }
    }
}