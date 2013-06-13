using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Models;

namespace TestControlTool.Management
{
    public static class Extensions
    {
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
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="obj">Object to seriailize</param>
        /// <param name="file">File name</param>
        /// <param name="extraTypes">Extra types to serialize</param>
        public static void SerializeToFile<T>(this T obj, string file, IEnumerable<Type> extraTypes)
        {
            using (var fileStream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var machinesListDeserializer = new XmlSerializer(typeof(T), extraTypes.ToArray());

                var xmlTextWriter = new XmlTextWriter(fileStream, Encoding.Unicode)
                {
                    Formatting = Formatting.Indented
                };

                machinesListDeserializer.Serialize(xmlTextWriter, obj);
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
    }
}
