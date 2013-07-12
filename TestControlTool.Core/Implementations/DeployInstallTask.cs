using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Models;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes task for the deploying machines
    /// </summary>
    public class DeployInstallTask : IChildTask
    {
        /// <summary>
        /// Xml file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Folder with logs
        /// </summary>
        public string ReportName { get; set; }
        
        /// <summary>
        /// Emits when child task got new output data
        /// </summary>
        public event OutputDataGot OutputDataGotHandler;

        /// <summary>
        /// Creates new task for the deploying machines
        /// </summary>
        /// <param name="fileName">Xml file for the task</param>
        /// <param name="reportName">Folder with logs</param>
        public DeployInstallTask(string fileName, string reportName)
        {
            FileName = fileName;
            ReportName = reportName;
        }

        /// <summary>
        /// Runs the child task
        /// </summary>
        public void Run()
        {
            var children = Extensions.DeserializeFromFile<DeployInstallTaskContainer>(FileName);

            var childTasks = new List<Task>();
            
            foreach (var child in children.Files)
            {
                IChildTask childTask = null;

                if (child.Key == VMServerType.VCenter)
                {
                    childTask = new VCenterDeployInstallTask
                        {
                            FileName = child.Value
                        };

                    childTask.OutputDataGotHandler += OutputDataGotHandler;
                }
                else if (child.Key == VMServerType.HyperV)
                {
                    childTask = new HyperVDeployInstallTask
                        {
                            FileName = child.Value,
                            ReportFolder = ConfigurationManager.AppSettings["LogsFolder"] + "\\" + ReportName
                        };
                }

                childTask.OutputDataGotHandler += OutputDataGotHandler;

                childTasks.Add(Task.Factory.StartNew(childTask.Run));
            }

            Task.WaitAll(childTasks.ToArray());
        }

        /// <summary>
        /// Stops the child task
        /// </summary>
        public void Stop()
        {
            var children = Extensions.DeserializeFromFile<DeployInstallTaskContainer>(FileName);
            
            foreach (var child in children.Files)
            {
                IChildTask childTask = null;

                if (child.Key == VMServerType.VCenter)
                {
                    childTask = new VCenterDeployInstallTask
                        {
                            FileName = child.Value
                        };

                    childTask.OutputDataGotHandler += OutputDataGotHandler;
                }
                else if (child.Key == VMServerType.HyperV)
                {
                    childTask = new HyperVDeployInstallTask
                        {
                            FileName = child.Value
                        };

                }

                childTask.OutputDataGotHandler += OutputDataGotHandler;

                childTask.Stop();
            }
        }
    }
}
