using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes task for the deploying machines
    /// </summary>
    public class VCenterDeployInstallTask : IChildTask
    {
        /// <summary>
        /// Xml file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Emits when child task got new output data
        /// </summary>
        public event OutputDataGot OutputDataGotHandler;

        /// <summary>
        /// Runs the child task
        /// </summary>
        public void Run()
        {
            var startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["AutoDeploymentConsole"], FileName + " " + "1")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

            var process = Process.Start(startInfo);

            if (OutputDataGotHandler != null)
            {
                process.OutputDataReceived += (obj, args) => OutputDataGotHandler(args.Data);
            }

            if (process == null)
            {
                throw new InvalidProgramException("Can't start process for autodeployment");
            }

            File.WriteAllText(FileName + ".process", process.Id.ToString());

            process.BeginOutputReadLine();

            process.WaitForExit();
        }

        /// <summary>
        /// Stops the child task
        /// </summary>
        public void Stop()
        {
            try
            {
                var processId = int.Parse(File.ReadAllText(FileName + ".process"));

                var process = Process.GetProcessById(processId);

                process.Kill();

                if (OutputDataGotHandler != null)
                {
                    OutputDataGotHandler("Process was terminated by request");
                }
            }
            catch (Exception e)
            {
                return;
            }
        }
    }
}
