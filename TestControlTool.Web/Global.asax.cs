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
using TestControlTool.Web.Models;

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
        /// Gets hash from the password
        /// </summary>
        public static readonly IPasswordHash PasswordHash = CastleResolver.Resolve<IPasswordHash>();

        /// <summary>
        /// Getting available tests for the trunk and release WGA test suites
        /// </summary>
        public static readonly TestSuiteTypesHelper TypesHelper = new TestSuiteTypesHelper();

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
            ModelBinders.Binders.Add(typeof(MachineModel), new MachineModelBinder());
        }
    }
}