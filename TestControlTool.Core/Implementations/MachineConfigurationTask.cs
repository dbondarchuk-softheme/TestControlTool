using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TestControlTool.Core.Helpers;
using TestControlTool.Core.Models;

namespace TestControlTool.Core.Implementations
{
    public class MachineConfigurationTask
    {
        /// <summary>
        /// Machine to configure
        /// </summary>
        public MachineConfigurationModel MachineConfigurationModel { get; set; }

        /// <summary>
        /// Runs the configuration task
        /// </summary>
        public void Run()
        {
            RunScript();
            CopyLog();
        }

        public string LogName
        {
            get { return ConfigurationManager.AppSettings["LogsFolder"] + "\\" + MachineConfigurationModel.ComputerName + ".log"; }
        }

        private string SharePath
        {
            get
            {
                return "\\\\" + MachineConfigurationModel.IPAddress + "\\" + MachineConfigurationModel.SharedFolderPath.Split('\\').Last();
            }
        }

        /*private void SetExecutionPolicy()
        {
            var arguments = "\\\\" + MachineConfigurationModel.IPAddress + " -u " + MachineConfigurationModel.AutoLogonUserName
                            + " -p " + MachineConfigurationModel.AutoLogonPassword + " powershell Set-ExecutionPolicy UnRestricted -force";

            var startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["PsExec"], arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

            var process = Process.Start(startInfo);

            process.OutputDataReceived += (obj, args) => Logger(args.Data);
            process.ErrorDataReceived += (obj, args) => Logger(args.Data);

            if (process == null)
            {
                throw new InvalidProgramException("Can't start process for set-execution policy");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }*/

        private void RunScript()
        {
            var arguments = "\\\\" + MachineConfigurationModel.IPAddress + " -u " + MachineConfigurationModel.AutoLogonUserName
                            + " -p " + MachineConfigurationModel.AutoLogonPassword + " " + "powershell -executionpolicy bypass -file \"" +
                            ConfigurationManager.AppSettings[
                                MachineConfigurationModel.MachineType == VMServerType.VCenter
                                    ? "VCenterMachineConfiguringScript"
                                    : "HyperVMachineConfiguringScript"]
                            + "\" \"" + MachineConfigurationModel.ComputerName + "\" \"" +
                            MachineConfigurationModel.AutoLogonUserName + "\" \"" + MachineConfigurationModel.AutoLogonPassword
                            + "\" \"" + MachineConfigurationModel.SharedFolderPath + "\" \"" +
                            MachineConfigurationModel.IPAddress + "\" \"" + MachineConfigurationModel.SubnetMask
                            + "\" \"" + MachineConfigurationModel.DefaultGateway + "\" \"" + MachineConfigurationModel.Dns1 +
                            "\" \"" + MachineConfigurationModel.Dns2
                            + "\" \"" + MachineConfigurationModel.TimeZoneName + "\"";

            var processId = ProcessAsUser.Launch(ConfigurationManager.AppSettings["PsExec"] + " " + arguments);

            var process = Process.GetProcessById(processId);
            
            process.WaitForExit(5*60*1000);
        }

        private void CopyLog()
        {
            try
            {
               /* ProcessAsUser.Launch("net use " + SharePath + " /user:" + MachineConfigurationModel.AutoLogonUserName +
                                     " " +
                                     MachineConfigurationModel.AutoLogonPassword);*/

                Process.Start("net", "use " + SharePath + " /user:" + MachineConfigurationModel.AutoLogonUserName +
                                     " " +
                                     MachineConfigurationModel.AutoLogonPassword);

                Thread.Sleep(2000);

                File.Copy(SharePath + "\\LOG.log", LogName, true);

                /*ProcessAsUser.Launch("net use " + SharePath + " /delete /y" +
                                     MachineConfigurationModel.AutoLogonPassword);*/

                Process.Start("net", "use " + SharePath + " /delete /y" +
                                     MachineConfigurationModel.AutoLogonPassword);
            }
            catch(Exception e)
            {
                File.WriteAllText(@"D:\error.txt", e.Message);
            }
        }
    }
}
