using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;

namespace TestControlTool.Core
{
    /// <summary>
    /// Different helpful extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Initial task message for logging
        /// </summary>
        public static readonly string InitialLogMessageFormat = "===========  {0}\t\t TaskName = \"{1}\" \t\t{2}  ===========";

        /// <summary>
        /// Get machine's server type from it's object
        /// </summary>
        /// <param name="machine">Machine's object</param>
        /// <returns>VMServerType</returns>
        public static VMServerType GetMachineType(this IMachine machine)
        {
            if (machine is VCenterMachine) return VMServerType.VCenter;

            if (machine is HyperVMachine) return VMServerType.HyperV;

            throw new NotSupportedException("Sorry, but this type doesn't supported");
        }

        /// <summary>
        /// Returns <see cref="occurrence"/>'s index of the char in the string
        /// </summary>
        /// <param name="str">String to look up</param>
        /// <param name="charToSearch">Char to search</param>
        /// <param name="occurrence">Number of char occurrence</param>
        /// <returns>Index of the char <see cref="occurrence"/>. If wasn't founded = -1</returns>
        public static int SpecificIndexOf(this string str, char charToSearch, int occurrence)
        {
            var count = 0;

            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == charToSearch)
                {
                    count++;

                    if (count == occurrence)
                    {
                        return i;
                    }
                }
                
            }

            return -1;
        }

        /// <summary>
        /// Serialize object to xml file
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="obj">Object to seriailize</param>
        /// <param name="file">File name</param>
        public static void SerializeToFile<T>(this T obj, string file)
        {
            using (var fileStream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var machinesListDeserializer = new XmlSerializer(typeof(T));

                var xmlTextWriter = new XmlTextWriter(fileStream, Encoding.Unicode)
                    {
                        Formatting = Formatting.Indented
                    };

                machinesListDeserializer.Serialize(xmlTextWriter, obj);
            }
        }

        /// <summary>
        /// Serialize object to xml file
        /// </summary>
        /// <param name="obj">Object to seriailize</param>
        /// <param name="file">File name</param>
        /// <param name="extraTypes">Extra types to serialize</param>
        public static void SerializeToFile(this object obj, string file, IEnumerable<Type> extraTypes)
        {
            using (var fileStream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var serializer = new XmlSerializer(obj.GetType(), extraTypes.ToArray());

                var xmlTextWriter = new XmlTextWriter(fileStream, Encoding.Unicode)
                    {
                        Formatting = Formatting.Indented
                    };

                serializer.Serialize(xmlTextWriter, obj);
            }
        }

        /// <summary>
        /// Deserialize object from the file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="file">File path</param>
        /// <returns>Object</returns>
        public static T DeserializeFromFile<T>(string file)
        {
            using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var deserializer = new XmlSerializer(typeof(T));

                return (T)deserializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserialize object from the file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="file">File path</param>
        /// <param name="extraTypes">Extra types for deserialize</param>
        /// <returns>Object</returns>
        public static T DeserializeFromFile<T>(string file, IEnumerable<Type> extraTypes)
        {
            using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var deserializer = new XmlSerializer(typeof(T), extraTypes.ToArray());

                return (T)deserializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserialize object from the file
        /// </summary>
        /// <param name="type">Type to deserialize</param>
        /// <param name="file">File path</param>
        /// <param name="extraTypes">Extra types for deserialize</param>
        /// <returns>Object</returns>
        public static object DeserializeFromFile(this Type type, string file, IEnumerable<Type> extraTypes)
        {
            using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var deserializer = new XmlSerializer(type, extraTypes.ToArray());

                return deserializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Creates needed empty files for the new task
        /// </summary>
        /// <param name="task">Task, which needed files</param>
        public static void CreateTaskFiles(this IScheduleTask task)
        {
            var childTasks = new Collection<ChildTaskModel>();
            childTasks.SerializeToFile(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + task.Id + ".xml");
        }

        /// <summary>
        /// Gets child container for the task
        /// </summary>
        /// <param name="task">Task to search</param>
        public static Collection<ChildTaskModel> GetTaskChildsFromFile(this IScheduleTask task)
        {
            return DeserializeFromFile<Collection<ChildTaskModel>>(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + task.Id + ".xml");
        }

        /// <summary>
        /// Saves child container for the to the xml files
        /// </summary>
        /// <param name="container">Child tasks</param>
        /// <param name="taskId">Task's id</param>
        public static void SaveTaskChildsToFile(this IEnumerable<ChildTaskModel> container, Guid taskId)
        {
            container.ToList().SerializeToFile(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + taskId + ".xml");
        }

        /// <summary>
        /// Removes all files for the task
        /// </summary>
        /// <param name="task">Task to remove</param>
        public static void DeleteTaskFiles(this IScheduleTask task)
        {
            var childs = task.GetTaskChildsFromFile();

            foreach (var child in childs)
            {
                try
                {
                    File.Delete(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + child.File);
                }
                catch
                {
                }
            }

            try
            {
                File.Delete(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + task.Id + ".xml");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Converts string to enum value
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <param name="enumType">Enum type</param>
        /// <returns>Enum value</returns>
        public static object ConvertToEnum(this string value, Type enumType)
        {
            if (!enumType.IsEnum) throw new NotSupportedException("T must be an Enum");

            try
            {
                return Enum.Parse(enumType, value);
            }
            catch
            {
                return Enum.GetValues(enumType).GetValue(0);
            }
        }

        /// <summary>
        /// Kill process and it's children by process id
        /// </summary>
        /// <param name="processId">Process id to kill</param>
        public static void KillProcessAndChildren(int processId)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + processId);
            var moc = searcher.Get();

            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        /// <summary>
        /// Waits until file is not created
        /// </summary>
        /// <param name="file">File path</param>
        /// <param name="sleep">Time to sleep between asking file existence</param>
        /// <param name="times">Count of attempts</param>
        public static void WaitForFileCreation(string file, TimeSpan sleep, int times)
        {
            var info = new FileInfo(file);

            for (var i = 0; i < times && !info.Exists; i++)
            {
                Thread.Sleep(sleep);
            }
        }
    }
}
