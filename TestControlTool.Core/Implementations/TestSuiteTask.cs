using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Helpers;

namespace TestControlTool.Core.Implementations
{
    public class TestSuiteTask : IChildTask
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
        /// Emits when child task got new output data
        /// </summary>
        public event OutputDataGot OutputDataGotHandler;

        /// <summary>
        /// Runs the child task
        /// </summary>
        public void Run()
        {
            var reportFolder = ConfigurationManager.AppSettings["LogsFolder"] + "\\" + ReportFileName;

            var appCmdLine = ConfigurationManager.AppSettings["TestPerformer"] + " Name \"" + reportFolder  + "\" // Load \"" + ConfigurationManager.AppSettings["TestPerformerScripts"] + "\" // Run \"" + FileName + "\" quiet";
            
            var processId = ProcessAsUser.Launch(appCmdLine);
            var process = Process.GetProcessById(processId);

            File.WriteAllText(FileName + ".process", process.Id.ToString());

            WaitForFolderCreation(reportFolder, new TimeSpan(0, 0, 1), 5);
            
            var watcher = new FileWatcher(reportFolder + "\\WebGuiAutomation.log");

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
                
                KillProcessAndChildren(processId);

                if (OutputDataGotHandler != null)
                {
                    OutputDataGotHandler("Process was terminated by request");
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private static void KillProcessAndChildren(int pid)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            var moc = searcher.Get();

            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                var process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private static void WaitForFolderCreation(string folder, TimeSpan sleep, int times)
        {
            var info = new DirectoryInfo(folder);

            for (var i = 0; i < times && !info.Exists; i++)
            {
                Thread.Sleep(sleep);    
            }
        }
    }
}
