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

            Collection<ChildTaskModel> childTasks;

            using (var mainFile = File.Open(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + id + ".xml", FileMode.Open))
            {
                var deserializer = new XmlSerializer(typeof(Collection<ChildTaskModel>));

                childTasks = (Collection<ChildTaskModel>)deserializer.Deserialize(mainFile);
            }
            
            foreach (var childTask in childTasks)
            {
                var text = File.ReadAllText(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + childTask.File, new UnicodeEncoding());

                foreach (var machine in _accountController.CachedMachines)
                {
                    foreach (var property in machine.GetType().GetProperties())
                    {
                        var token = string.Format("${0}/{1}", machine.Id, property.Name);
                        var replace = string.Format("{0}", property.GetValue(machine));

                        text = text.Replace("{" + token + "}", replace);
                    }
                }

                var newFile = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + childTask.File + ".new";

                File.WriteAllText(newFile, text, new UnicodeEncoding());

                IChildTask task = null;

                switch (childTask.TaskType)
                {
                    case TaskType.DeployInstall:
                        task = new DeployInstallTask
                            {
                                FileName = newFile
                            };

                        break;

                    case TaskType.TestSuite:
                        task = new TestSuiteTask
                            {
                                FileName = newFile,
                                ReportFileName = id + "." + childTask.Name
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
            Console.WriteLine(message);
            logger.Info(childTaskName + ".\t" + message);
        }
    }
}
