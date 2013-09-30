using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;

namespace TestControlTool.UpdateService
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            string directoriesString, assemblyName, baseType, emailSubject, emailBody;

            if (args.Length != 5)
            {
                directoriesString = ConfigurationManager.AppSettings["Directories"];
                assemblyName = ConfigurationManager.AppSettings["AssemblyName"];
                baseType = ConfigurationManager.AppSettings["BaseType"];
                emailSubject = ConfigurationManager.AppSettings["EmailSubject"];
                emailBody = ConfigurationManager.AppSettings["EmailBody"];
            }
            else
            {
                directoriesString = args[0];
                assemblyName = args[1];
                baseType = args[2];
                emailSubject = args[3];
                emailBody = args[4];
            }

            var directories = directoriesString.Split(';').Select(x => new KeyValuePair<string, string>(FindNewestSubDirectory(x.Split(',')[0]), x.Split(',')[1]));

            AssemblyLocator.Init();

            var secondAssembly = Path.GetFullPath("Temporary." + assemblyName);
            File.Copy(directories.First().Value + "\\" + assemblyName, secondAssembly, true);

            var job = new JobContainer(new IJob[]
                {
                    new CheckAssembliesDifferneceJob
                        {
                            FirstAssembly = directories.First().Key + "\\" + assemblyName,
                            SecondAssembly = secondAssembly,
                            BaseType = baseType,
                            OnDifference = difference =>
                                {
                                    var client = new TaskWcfServiceClient();
                                    client.Open();

                                    var body = GenerateMessageBody(emailBody, difference);
                                    
                                    client.SendEmailToAll(emailSubject, body);

                                    client.Close();
                                }
                        },

                        
                    new StartStopServiceJob("WAS", StartStopServiceJob.Command.Stop),

                    new StartStopServiceJob("TestControlTools.SchedulerService", StartStopServiceJob.Command.Stop),
                    
                    new CopyJob(directories),

                    new StartStopServiceJob("TestControlTools.SchedulerService", StartStopServiceJob.Command.Start),

                    new StartStopServiceJob("W3SVC", StartStopServiceJob.Command.Start)
                });

            job.Run();

            //File.Delete(secondAssembly);
        }

        private static string FindNewestSubDirectory(string directory)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directory);

                return directoryInfo.GetDirectories().OrderByDescending(x => x.LastAccessTime).First().FullName;

            }
            catch (InvalidOperationException)
            {
                Logger.Error("Folder '" + directory + "' doesn't contain any subfolder");

                throw;
            }
            catch(DirectoryNotFoundException)
            {
                Logger.Error("Folder '" + directory + "' doesn't exist");

                throw;
            }
        }

        private static string GenerateMessageBody(string emailBody, AssembliesDifference difference)
        {
            var addedClasses = difference.ClassesDifference != null ? difference.ClassesDifference.AddedClasses.Aggregate("", (s, s1) => s + s1 + "\n").Trim() : "--None--";
            var removedClasses = difference.ClassesDifference != null ? difference.ClassesDifference.RemovedClasses.Aggregate("", (s, s1) => s + s1 + "\n").Trim() : "--None--";

            var addedProperties = difference.PropertiesDifference != null ? difference.PropertiesDifference.AddedProperties.Aggregate("", (s, pair) => s + pair.Key + " - " + pair.Value + "\n").Trim() : "--None--";
            var removedProperties = difference.PropertiesDifference != null ? difference.PropertiesDifference.RemovedProperties.Aggregate("", (s, pair) => s + pair.Key + " - " + pair.Value + "\n").Trim() : "--None--";

            return string.Format(CultureInfo.InvariantCulture, emailBody, addedClasses, removedClasses, addedProperties, removedProperties).Replace(@"\n", Environment.NewLine);
        }
    }
}
