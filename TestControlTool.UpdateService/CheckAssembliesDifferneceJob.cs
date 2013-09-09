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
            
            var firstTypes = firstAssembly.ExportedTypes.Where(x => !string.IsNullOrEmpty(BaseType) && x.BaseType != null && x.BaseType.Name == BaseType).Select(x => x.FullName).ToList();
            var secondTypes = secondAssembly.ExportedTypes.Where(x => !string.IsNullOrEmpty(BaseType) && x.BaseType != null && x.BaseType.Name == BaseType).Select(x => x.FullName).ToList();
            
            if (firstTypes.SequenceEqual(secondTypes))
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
                        AddedClasses = secondTypes.Except(firstTypes),
                        RemovedClasses = firstTypes.Except(secondTypes)
                    };

                    OnDifference(difference);
                }
            }
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
        /// Added classes in the second assemblies (Full Names)
        /// </summary>
        public IEnumerable<string> AddedClasses { get; set; }

        /// <summary>
        /// Removed classes in the second assemblies (Full Names)
        /// </summary>
        public IEnumerable<string> RemovedClasses { get; set; }
    }
}
