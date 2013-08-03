using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Models;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Parses tasks's child jobs from the xml files
    /// </summary>
    public class XmlTaskParser : ITaskParser
    {
        private readonly IAccountController _accountController;

        public XmlTaskParser(IAccountController accountController)
        {
            _accountController = accountController;
        }

        /// <summary>
        /// Parses tasks's child jobs from the xml files
        /// </summary>
        /// <param name="id">Id of the task</param>
        /// <param name="logger">Logger for the task</param>
        /// <returns>List of child tasks</returns>
        public IEnumerable<IChildTask> Parse(Guid id, ILogger logger)
        {
            var result = new List<IChildTask>();

            var childTasks = Extensions.DeserializeFromFile<Collection<ChildTaskModel>>(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + id + ".xml");
            
            foreach (var childTask in childTasks)
            {
                var newFile = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + childTask.File;
                
                IChildTask task = null;

                switch (childTask.TaskType)
                {
                    case TaskType.DeployInstall:
                        ParseAutoDeployFiles(newFile);

                        task = new DeployInstallTask(newFile + ".new", id + "." + childTask.Name)
                            {
                                Name = childTask.Name
                            };

                        break;

                    case TaskType.TestSuiteTrunk:
                        ParseTestPerformerFiles(newFile);

                        task = new TestSuiteTrunkTask(newFile + ".new", id + "." + childTask.Name)
                            {
                                Name = childTask.Name
                            };
                        break;

                    case TaskType.TestSuiteRelease:
                        ParseTestPerformerFiles(newFile);

                        task = new TestSuiteReleaseTask(newFile + ".new", id + "." + childTask.Name)
                            {
                                Name = childTask.Name
                            };

                        break;
                }
                
                task.OutputDataGotHandler += output => Logger(logger, childTask.TaskType.ToString(), output);

                result.Add(task);
            }

            return result;
        }

        private static void Logger(ILogger logger, string childTaskName, string message)
        {
            logger.Message(message.Trim());
        }

        /// <summary>
        /// Parsing autodeploy files
        /// </summary>
        /// <param name="fileName">File with autodeploy container</param>
        /// <param name="newExtension">New extension, which will be added to the file name. Default = ".new"</param>
        public void ParseAutoDeployFiles(string fileName, string newExtension = ".new")
        {
            var children = Extensions.DeserializeFromFile<DeployInstallTaskContainer>(fileName);
            var newContainer = new DeployInstallTaskContainer()
                {
                    Machines = children.Machines,
                    Files = new List<Pair<VMServerType, string>>()
                };

            foreach (var child in children.Files)
            {
                var text = File.ReadAllText(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + child.Value, new UnicodeEncoding());

                foreach (var machine in _accountController.CachedMachines)
                {
                    foreach (var property in machine.GetType().GetProperties())
                    {
                        var token = string.Format("${0}/{1}", machine.Id, property.Name);
                        var replace = string.Format("{0}", property.GetValue(machine));

                        text = text.Replace("{" + token + "}", replace);
                    }
                }

                foreach (var server in _accountController.CachedServers)
                {
                    foreach (var property in server.GetType().GetProperties())
                    {
                        var token = string.Format("${0}/{1}", server.Id, property.Name);
                        var replace = string.Format("{0}", property.GetValue(server));

                        text = text.Replace("{" + token + "}", replace);
                    }
                }

                var newFile = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + child.Value + newExtension;

                File.WriteAllText(newFile, text, new UnicodeEncoding());

                newContainer.Files.Add(new Pair<VMServerType, string>(child.Key, newFile));
            }

            newContainer.SerializeToFile(fileName + newExtension);
        }

        /// <summary>
        /// Parsing test performer files
        /// </summary>
        /// <param name="fileName">File with WGA test suite</param>
        /// <param name="newExtension">New extension, which will be added to the file name. Default = ".new"</param>
        public void ParseTestPerformerFiles(string fileName, string newExtension = ".new")
        {
            var text = File.ReadAllText(fileName, new UnicodeEncoding());

            foreach (var machine in _accountController.CachedMachines)
            {
                foreach (var property in machine.GetType().GetProperties())
                {
                    var token = string.Format("${0}/{1}", machine.Id, property.Name);
                    var replace = string.Format("{0}", property.GetValue(machine));

                    text = text.Replace("{" + token + "}", replace);
                }
            }
            
            File.WriteAllText(fileName + newExtension, text, new UnicodeEncoding());
        }
    }
}