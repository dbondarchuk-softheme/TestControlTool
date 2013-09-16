using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestControlTool.Web
{
    public enum TestSuiteType
    {
        BackendTrunk,
        BackendRelease,
        UITrunk,
        UIRelease
    }

    /// <summary>
    /// Getting available tests for the trunk and release WGA test suites
    /// </summary>
    public static class TestSuiteTypesHelper
    {
        public static IEnumerable<Type> GetScriptsTypes(TestSuiteType type)
        {
            var assemblyPath = GetScriptsAssemblyPath(type);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));

            var types = new List<Type>();

            var assembly = Assembly.LoadFile(assemblyPath);

            types.AddRange(assembly.ExportedTypes);

            var referencedAssemblies = assembly.GetReferencedAssemblies().Where(x => x.FullName.Contains("TestPerformerCore") || x.FullName.Contains("Implementation")).Select(Assembly.Load);
            types.AddRange(referencedAssemblies.SelectMany(x => x.ExportedTypes));
            
            return types;
        }

        public static IEnumerable<Type> GetOnlyScriptsTypes(TestSuiteType type)
        {
            var assemblyPath = GetScriptsAssemblyPath(type);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));

            var types = new List<Type>();

            var assembly = Assembly.LoadFile(assemblyPath);

            types.AddRange(assembly.ExportedTypes);
            
            return types;
        }

        public static IEnumerable<Type> GetAvailabaleTests(TestSuiteType type)
        {
            return GetScriptsTypes(type).Where(x => x.BaseType != null && x.BaseType.Name == "AppAssureTest");
        }

        public static IEnumerable<Type> GetTestPerformerTypes(TestSuiteType type)
        {
            var assemblyPath = GetTestPerformerAssemblyPath(type);

            var types = new List<Type>();

            var assembly = Assembly.LoadFile(assemblyPath);

            types.AddRange(assembly.ExportedTypes);

            var referencedAssemblies = assembly.GetReferencedAssemblies().Where(x => x.FullName.StartsWith("WebGuiAutomation")).Select(Assembly.Load);
            types.AddRange(referencedAssemblies.SelectMany(x => x.ExportedTypes));

            return types;
        }

        private static string GetScriptsAssemblyPath(TestSuiteType type)
        {
            var assembly = "";

            switch (type)
            {
                case TestSuiteType.UITrunk:
                    assembly = ConfigurationManager.AppSettings["TestPerformerScripts"];
                    break;

                case TestSuiteType.UIRelease:
                    assembly = ConfigurationManager.AppSettings["TestPerformerReleaseScripts"];
                    break;

                case TestSuiteType.BackendTrunk:
                    assembly = ConfigurationManager.AppSettings["TestPerformerScripts"];
                    break;

                case TestSuiteType.BackendRelease:
                    assembly = ConfigurationManager.AppSettings["TestPerformerReleaseScripts"];
                    break;
            }
            
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

        private static string GetTestPerformerAssemblyPath(TestSuiteType type)
        {
            var assembly = "";

            switch (type)
            {
                case TestSuiteType.UITrunk:
                    assembly = ConfigurationManager.AppSettings["TestPerformer"];
                    break;

                case TestSuiteType.UIRelease:
                    assembly = ConfigurationManager.AppSettings["TestPerformerRelease"];
                    break;

                case TestSuiteType.BackendTrunk:
                    assembly = ConfigurationManager.AppSettings["TestPerformer"];
                    break;

                case TestSuiteType.BackendRelease:
                    assembly = ConfigurationManager.AppSettings["TestPerformerRelease"];
                    break;
            }

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