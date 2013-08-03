using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    public class MachineConfigurationTask
    {
        /// <summary>
        /// Machine to configure
        /// </summary>
        public IMachine Machine { get; set; }

        /// <summary>
        /// Runs the configuration task
        /// </summary>
        public void Run()
        {
            File.WriteAllText(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + Machine.Id + ".ConfigurationTask.percentage", "50");
        }

        /// <summary>
        /// Gets the status of the configuring
        /// </summary>
        /// <param name="machineId">Machine's id</param>
        /// <returns>Percantage of the process or -1, if ta</returns>
        public static int GetStatus(Guid machineId)
        {
            try
            {
                var fileInfo = new FileInfo(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + machineId + ".ConfigurationTask.percentage");

                if (!fileInfo.Exists || (DateTime.UtcNow - fileInfo.LastAccessTimeUtc > new TimeSpan(3, 0, 0)))
                {
                    return -1;
                }

                return int.Parse(File.ReadAllText(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + machineId + ".ConfigurationTask.percentage"));
            }
            catch
            {
                return -1;
            }
        }
    }
}
