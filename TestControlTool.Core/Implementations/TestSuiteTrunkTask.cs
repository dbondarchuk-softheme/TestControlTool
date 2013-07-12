using System.Configuration;
using TestControlTool.Core.Contracts;

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
    }
}
