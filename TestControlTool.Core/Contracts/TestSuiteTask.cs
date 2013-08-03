using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using TestControlTool.Core.Helpers;

namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Basic class for WGA test suites task
    /// </summary>
    public abstract class TestSuiteTask : IChildTask
    {
        /// <summary>
        /// Xml file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File of the report
        /// </summary>
        public string ReportFileName { get; set; }

        /// <summary>
        /// Name of the task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Emits when child task got new output data
        /// </summary>
        public event OutputDataGot OutputDataGotHandler;

        protected string ReportFolder = "";

        protected string AppCmdLine = "";

        /// <summary>
        /// Runs the child task
        /// </summary>
        public void Run()
        {
            if (!string.IsNullOrWhiteSpace(ReportFolder)) Directory.CreateDirectory(ReportFolder);

            var processId = ProcessAsUser.Launch(AppCmdLine);
            var process = Process.GetProcessById(processId);

            File.WriteAllText(FileName + ".process", process.Id.ToString(CultureInfo.InvariantCulture));

            var logFile = ReportFolder + "\\" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "__LOG.log";

            Extensions.WaitForFileCreation(logFile, new TimeSpan(0, 0, 1), 5);

            var watcher = new FileWatcher(logFile);

            if (OutputDataGotHandler != null)
            {
                watcher.FileChanged += (file, text) => OutputDataGotHandler(text);
            }

            process.WaitForExit();

            watcher.Dispose();
        }

        /// <summary>
        /// Stops the child task
        /// </summary>
        public void Stop()
        {
            try
            {
                var processId = int.Parse(File.ReadAllText(FileName + ".process"));

                Extensions.KillProcessAndChildren(processId);

                if (OutputDataGotHandler != null)
                {
                    OutputDataGotHandler("Process was terminated by request");
                }
            }
            catch (Exception)
            {
            }
        }

        protected Dictionary<string, string> GetMachineInfo()
        {
            var result = new Dictionary<string, string>();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(FileName);

            var node = xmlDocument.SelectSingleNode("/Suite/Machine");

            if (node == null || node.Attributes == null) throw new FormatException("Wrong file format. Node Suite/Machine does not exist");

            result.Add("address", node.Attributes["address"].Value);
            result.Add("username", node.Attributes["username"].Value);
            result.Add("password", node.Attributes["password"].Value);
            result.Add("share", node.Attributes["share"].Value);

            return result;
        } 
    }
}
