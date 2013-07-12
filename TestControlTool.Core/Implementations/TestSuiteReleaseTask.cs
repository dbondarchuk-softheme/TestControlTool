using System.Configuration;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Task for WGA release
    /// </summary>
    public class TestSuiteReleaseTask : TestSuiteTask
    {
        public TestSuiteReleaseTask(string fileName, string reportFileName)
        {
            FileName = fileName;
            ReportFileName = reportFileName;

            ReportFolder = ConfigurationManager.AppSettings["LogsFolder"] + "\\" + ReportFileName;

            var machineInfo = GetMachineInfo();

            AppCmdLine = 
                //ConfigurationManager.AppSettings["PsExec"] + " -u " + ConfigurationManager.AppSettings["PsExecUserName"] + " -p " + ConfigurationManager.AppSettings["PsExecUserPassword"] + " " + 
                "powershell \"" + ConfigurationManager.AppSettings["TestPerformerReleaseRunScript"] + "\" \"" + machineInfo["address"] + "\" \"" + machineInfo["username"]
                + "\" \"" + machineInfo["password"] + "\" \"" + machineInfo["share"] + "\" \"" + FileName
                + "\" \"" + ConfigurationManager.AppSettings["PsExec"] + "\" \"" + ReportFolder + "\"";
        }
    }
}
