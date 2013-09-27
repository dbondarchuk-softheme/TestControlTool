using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;

namespace TestControlTool.UpdateService
{
    /// <summary>
    /// Checks difference between two assemblies exported types
    /// </summary>
    public class CheckAssembliesDifferneceJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// First assembly
        /// </summary>
        public string FirstAssembly { get; set; }

        /// <summary>
        /// Second assembly
        /// </summary>
        public string SecondAssembly { get; set; }

        /// <summary>
        /// If not null - looking only for classes which are inherited from this base class name. Should be provided as a short name
        /// </summary>
        public string BaseType { get; set; }

        /// <summary>
        /// If two assemblies are different
        /// </summary>
        public OnDifferenceDelegate OnDifference;

        /// <summary>
        /// If two assemblies are same
        /// </summary>
        public OnSameDelegate OnSame;

        /// <summary>
        /// Starts the job
        /// </summary>
        public void Run()
        {
            Logger.Info("Getting types..");

            var firstAssembly = Assembly.LoadFrom(FirstAssembly);
            var secondAssembly = Assembly.LoadFile(SecondAssembly);

            var firstTypes = firstAssembly.ExportedTypes.Where(x => !string.IsNullOrEmpty(BaseType) && x.BaseType != null && x.BaseType.Name == BaseType).ToList();
            var secondTypes = secondAssembly.ExportedTypes.Where(x => !string.IsNullOrEmpty(BaseType) && x.BaseType != null && x.BaseType.Name == BaseType).ToList();

            var classesDifference = CheckClassesDifference(firstTypes, secondTypes);
            var propertiesDifference = CheckPropertiesDifference(firstTypes, secondTypes, classesDifference);

            if (classesDifference == null && propertiesDifference == null)
            {
                Logger.Info("Two assemblies are equal");

                if (OnSame != null)
                {
                    OnSame();
                }
            }
            else
            {
                Logger.Info("Two assemblies aren't equal");

                if (OnDifference != null)
                {
                    var difference = new AssembliesDifference
                        {
                            FirstAssembly = firstAssembly,
                            SecondAssembly = secondAssembly,
                            ClassesDifference = classesDifference,
                            PropertiesDifference = propertiesDifference
                        };

                    OnDifference(difference);
                }
            }
        }

        private ClassesDifference CheckClassesDifference(IEnumerable<Type> firstTypes, IEnumerable<Type> secondTypes)
        {
            var firstNames = firstTypes.Select(x => x.FullName).ToList();
            var secondNames = secondTypes.Select(x => x.FullName).ToList();

            if (firstNames.SequenceEqual(secondNames))
            {
                Logger.Info("Two assemblies don't have difference between classes existence");

                return null;
            }

            Logger.Info("Two assemblies have difference between classes existence");

            return new ClassesDifference
                {
                    AddedClasses = secondNames.Except(firstNames),
                    RemovedClasses = firstNames.Except(secondNames)
                };
        }

        private PropertiesDifference CheckPropertiesDifference(IEnumerable<Type> firstTypes, IEnumerable<Type> secondTypes, ClassesDifference classesDifference = null)
        {
            var leftTypes = firstTypes.Where(x => classesDifference == null || (!classesDifference.RemovedClasses.Contains(x.FullName))).ToDictionary(x => x.FullName, x => x);
            var rightTypes = secondTypes.Where(x => classesDifference == null || (!classesDifference.AddedClasses.Contains(x.FullName))).ToDictionary(x => x.FullName, x => x);

            var types = leftTypes.Join(rightTypes, pair => pair.Key, pair => pair.Key,
                                       (leftPair, rightPair) => new KeyValuePair<Type, Type>(leftPair.Value, rightPair.Value));

            var addedProperties = new List<KeyValuePair<string, string>>();
            var removedProperties = new List<KeyValuePair<string, string>>();

            foreach (var pair in types)
            {
                var leftProperties = pair.Key.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.CanRead).ToList();
                var rightProperties = pair.Value.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.CanRead).ToList();

                removedProperties.AddRange(leftProperties.Where(property => !rightProperties.Any(x => x.Name == property.Name)).Select(property => new KeyValuePair<string, string>(pair.Key.FullName, property.Name)));

                addedProperties.AddRange(rightProperties.Where(property => !leftProperties.Any(x => x.Name == property.Name)).Select(property => new KeyValuePair<string, string>(pair.Value.FullName, property.Name)));
            }

            if (!addedProperties.Any() && !removedProperties.Any())
            {
                Logger.Info("Two assemblies don't have difference between properties existence");

                return null;
            }

            Logger.Info("Two assemblies have difference between properties existence");

            return new PropertiesDifference
                {
                    AddedProperties = addedProperties,
                    RemovedProperties = removedProperties
                };
        }
    }

    public delegate void OnDifferenceDelegate(AssembliesDifference difference);

    public delegate void OnSameDelegate();

    /// <summary>
    /// Contains information about difference between two assemblies
    /// </summary>
    public class AssembliesDifference
    {
        /// <summary>
        /// First assembly
        /// </summary>
        public Assembly FirstAssembly { get; set; }

        /// <summary>
        /// Second assembly
        /// </summary>
        public Assembly SecondAssembly { get; set; }

        /// <summary>
        /// Difference between existing of classes in two assemblies
        /// </summary>
        public ClassesDifference ClassesDifference { get; set; }

        /// <summary>
        /// Difference between existing of classes' properties in two assemblies
        /// </summary>
        public PropertiesDifference PropertiesDifference { get; set; }
    }

    public class ClassesDifference
    {
        /// <summary>
        /// Added classes in the second assemblies (Full Names)
        /// </summary>
        public IEnumerable<string> AddedClasses { get; set; }

        /// <summary>
        /// Removed classes in the second assemblies (Full Names)
        /// </summary>
        public IEnumerable<string> RemovedClasses { get; set; }
    }

    public class PropertiesDifference
    {
        /// <summary>
        /// Added properties from classes in the second classes. Pair = Class (Full Name) - Property Name
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> AddedProperties { get; set; }

        /// <summary>
        /// Removed properties from classes in the second classes. Pair = Class (Full Name) - Property Name
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> RemovedProperties { get; set; } 
    }
}
