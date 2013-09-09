using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace TestControlTool.UpdateService
{
    /// <summary>
    /// Works with several jobs synchronously
    /// </summary>
    internal class JobContainer : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnumerable<IJob> _jobs;

        public JobContainer(IEnumerable<IJob> jobs)
        {
            _jobs = jobs;
        } 

        /// <summary>
        /// Starts the jobs
        /// </summary>
        public void Run()
        {
            foreach (var job in _jobs)
            {
                try
                {
                    job.Run();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }
        }
    }
}
