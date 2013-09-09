using System.Configuration;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Helpers;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Task for WGA trunk
    /// </summary>
    public class TestSuiteTrunkTask : TestSuiteTask
    {
        public TestSuiteTrunkTask(string fileName, string reportFileName)
        {
            FileName = fileName;
            ReportFileName = reportFileName;

            ReportFolder = ConfigurationManager.AppSettings["LogsFolder"] + "\\" + ReportFileName;
            
            var machineInfo = GetMachineInfo();

            AppCmdLine = 
                //ConfigurationManager.AppSettings["PsExec"] + " -u " + ConfigurationManager.AppSettings["PsExecUserName"] + " -p " + ConfigurationManager.AppSettings["PsExecUserPassword"] + " " +
                "powershell \"" + ConfigurationManager.AppSettings["TestPerformerRunScript"] + "\" \"" + machineInfo["address"] + "\" \"" + machineInfo["username"] 
                + "\" \"" + machineInfo["password"] + "\" \"" + machineInfo["share"] + "\" \"" + FileName 
                + "\" \"" + ConfigurationManager.AppSettings["PsExec"] + "\" \"" + ReportFolder +"\"";
        }

        public override void Stop()
        {
            var machineInfo = GetMachineInfo();

            var commandLine = ConfigurationManager.AppSettings["PsExec"] + " \\\\" + machineInfo["address"] + " -u " + machineInfo["username"]
                + " -p " + machineInfo["password"] + " -c -f " + ConfigurationManager.AppSettings["ProcessKiller"] + " WebGuiAutomation.TestPerformer";

            ProcessAsUser.Launch(commandLine);

            base.Stop();
        }
    }
}
