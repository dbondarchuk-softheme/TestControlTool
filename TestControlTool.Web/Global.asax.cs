using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BootstrapSupport;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Web.App_Start;

namespace TestControlTool.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class TestControlToolApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Account controller
        /// </summary>
        public static readonly IAccountController AccountController = CastleResolver.Resolve<IAccountController>();
        
        /// <summary>
        /// Available WebGuiTests
        /// </summary>
        public static readonly IEnumerable<Type> AvailableTests = GetAvailabaleTests();

        /// <summary>
        /// All TestPerformer types
        /// </summary>
        public static readonly IEnumerable<Type> TestPerformerTypes = GetTestPerformerTypes();

        /// <summary>
        /// Gets hash from the password
        /// </summary>
        public static readonly IPasswordHash PasswordHash = CastleResolver.Resolve<IPasswordHash>();

        protected void Application_Start()
        {
            ViewEngines.Engines.Add(new RazorViewEngine());
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            BootstrapBundleConfig.RegisterBundles(System.Web.Optimization.BundleTable.Bundles);
            BootstrapMvcSample.ExampleLayoutsRouteConfig.RegisterRoutes(RouteTable.Routes);
            ModelBinders.Binders.Add(typeof(string), new TrimModelBinder());
        }

        private static IEnumerable<Type> GetAvailabaleTests()
        {
            var assemblyPath = GetAssemblyPath();

            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));

            var dir = Directory.GetCurrentDirectory();

            var assembly = Assembly.LoadFile(assemblyPath);

            var tests = assembly.ExportedTypes.Where(x => x.BaseType != null && x.BaseType.Name == "AppAssureTest");
            return tests;
        }

        private static IEnumerable<Type> GetTestPerformerTypes()
        {
            var assemblyPath = GetAssemblyPath();

            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));

            var dir = Directory.GetCurrentDirectory();

            var assembly = Assembly.LoadFile(assemblyPath);

            var types = assembly.ExportedTypes;
            return types;
        }

        private static string GetAssemblyPath()
        {
            var assembly = ConfigurationManager.AppSettings["TestPerformerScripts"];

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