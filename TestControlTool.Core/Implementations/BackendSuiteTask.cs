using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Backend automation tests
    /// </summary>
    public class BackendSuiteTask : IChildTask
    {
        /// <summary>
        /// List of already used ports by Fitnesse 
        /// </summary>
        private static ConcurrentBag<int> _usedPorts = new ConcurrentBag<int>();

        /// <summary>
        /// Name of the suite to run
        /// </summary>
        public string SuiteName { get; set; }

        /// <summary>
        /// Emits when child task got new output data
        /// </summary>
        public event OutputDataGot OutputDataGotHandler;

        /// <summary>
        /// Runs the child task
        /// </summary>
        public void Run()
        {
            var port = _usedPorts.Count > 0 ? _usedPorts.Last() + 1 : 8088;
            _usedPorts.Add(port);

            try
            {
                var startInfo = new ProcessStartInfo("java.exe", "-jar fitnesse.jar -c " + SuiteName + "?suite -p " + port)
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

                process.BeginOutputReadLine();

                process.WaitForExit();
            }
            finally
            {
                lock (_usedPorts)
                {
                    _usedPorts = new ConcurrentBag<int>(_usedPorts.Where(x => x != port));
                }
            }
        }

        /// <summary>
        /// Stops the child task
        /// </summary>
        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}
