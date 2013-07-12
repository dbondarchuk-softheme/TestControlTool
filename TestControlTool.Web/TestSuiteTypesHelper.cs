using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestControlTool.Web
{
    /// <summary>
    /// Getting available tests for the trunk and release WGA test suites
    /// </summary>
    public class TestSuiteTypesHelper
    {
        /// <summary>
        /// Available WGA trunk tests
        /// </summary>
        public readonly IEnumerable<Type> AvailableTrunkTests = GetAvailabaleTests(true);

        /// <summary>
        /// Available WGA release tests
        /// </summary>
        public readonly IEnumerable<Type> AvailableReleaseTests = GetAvailabaleTests(false);

        /// <summary>
        /// All TestPerformer trunk types
        /// </summary>
        public readonly IEnumerable<Type> TestPerformerTrunkTypes = GetTestPerformerTypes(true);

        /// <summary>
        /// All TestPerformer release types
        /// </summary>
        public readonly IEnumerable<Type> TestPerformerReleaseTypes = GetTestPerformerTypes(false);

        /// <summary>
        /// All Scripts trunk types
        /// </summary>
        public readonly IEnumerable<Type> ScriptsTrunkTypes = GetScriptsTypes(true);

        /// <summary>
        /// All Scripts release types
        /// </summary>
        public readonly IEnumerable<Type> ScriptsReleaseTypes = GetScriptsTypes(false);

        private static IEnumerable<Type> GetScriptsTypes(bool trunk)
        {
            var assemblyPath = GetScriptsAssemblyPath(trunk);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));

            var types = new List<Type>();

            var assembly = Assembly.LoadFile(assemblyPath);

            types.AddRange(assembly.ExportedTypes);

            var referencedAssemblies = assembly.GetReferencedAssemblies().Where(x => x.FullName.StartsWith("WebGuiAutomation.TestPerformerCore")).Select(Assembly.Load);
            types.AddRange(referencedAssemblies.SelectMany(x => x.ExportedTypes));
            
            return assembly.ExportedTypes;
        }

        private static IEnumerable<Type> GetAvailabaleTests(bool trunk)
        {
            return GetScriptsTypes(trunk).Where(x => x.BaseType != null && x.BaseType.Name == "AppAssureTest");
        }

        private static IEnumerable<Type> GetTestPerformerTypes(bool trunk)
        {
            var assemblyPath = GetTestPerformerAssemblyPath(trunk);

            var types = new List<Type>();

            var assembly = Assembly.LoadFile(assemblyPath);

            types.AddRange(assembly.ExportedTypes);

            var referencedAssemblies = assembly.GetReferencedAssemblies().Where(x => x.FullName.StartsWith("WebGuiAutomation")).Select(Assembly.Load);
            types.AddRange(referencedAssemblies.SelectMany(x => x.ExportedTypes));

            return types;
        }

        private static string GetScriptsAssemblyPath(bool trunk)
        {
            var assembly = trunk ? ConfigurationManager.AppSettings["TestPerformerScripts"] : ConfigurationManager.AppSettings["TestPerformerReleaseScripts"];

            var serverAssemblyFile = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath.Replace('/', '\\'));

            if (assembly.StartsWith(@"..\"))
            {
                var executionPath = Directory.GetDirectoryRoot(serverAssemblyFile);

                assembly = executionPath + "\\" + assembly.Remove(0, 2);
            }
            else if (assembly.StartsWith(@".\"))
            {
                assembly = serverAssemblyFile + "\\" + assembly.Remove(0, 2);
            }

            return assembly;
        }

        private static string GetTestPerformerAssemblyPath(bool trunk)
        {
            var assembly = trunk ? ConfigurationManager.AppSettings["TestPerformer"] : ConfigurationManager.AppSettings["TestPerformerRelease"];

            var serverAssemblyFile = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath.Replace('/', '\\'));

            if (assembly.StartsWith(@"..\"))
            {
                var executionPath = Directory.GetDirectoryRoot(serverAssemblyFile);

                assembly = executionPath + "\\" + assembly.Remove(0, 2);
            }
            else if (assembly.StartsWith(@".\"))
            {
                assembly = serverAssemblyFile + "\\" + assembly.Remove(0, 2);
            }

            return assembly;
        }
    }
}