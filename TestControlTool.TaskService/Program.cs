using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TestControlTool.TaskService
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            var servicesToRun = new ServiceBase[] 
                { 
                    new SchedulerService() 
                };

            ServiceBase.Run(servicesToRun);
        }
    }
}
