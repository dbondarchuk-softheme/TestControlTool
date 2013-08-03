using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Helpers;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes task for the deploying machines
    /// </summary>
    public class HyperVDeployInstallTask : IChildTask
    {
        /// <summary>
        /// Xml file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Xml file
        /// </summary>
        public string ReportFolder { get; set; }

        /// <summary>
        /// Name of the task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Emits when child task got new output data
        /// </summary>
        public event OutputDataGot OutputDataGotHandler;

        /// <summary>
        /// Runs the child task
        /// </summary>
        public void Run()
        {
            if (!string.IsNullOrWhiteSpace(ReportFolder)) Directory.CreateDirectory(ReportFolder);
            
            var arguments = "powershell \"" + ConfigurationManager.AppSettings["HyperVDeploySctipt"] + "\" 1 \"" + FileName + "\" \"" + ReportFolder + "\"";

            var processId = ProcessAsUser.Launch(arguments);
            var process = Process.GetProcessById(processId);

            File.WriteAllText(FileName + ".process", process.Id.ToString(CultureInfo.InvariantCulture));

            var logFile = ReportFolder + "\\" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "__LOG.log";

            Extensions.WaitForFileCreation(logFile, new TimeSpan(0, 0, 1), 10);
            
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
    }
}
