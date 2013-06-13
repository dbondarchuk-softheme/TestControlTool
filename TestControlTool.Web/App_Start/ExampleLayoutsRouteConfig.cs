using System.Web.Routing;
using NavigationRoutes;
using TestControlTool.Web.Controllers;

namespace BootstrapMvcSample
{
    public class ExampleLayoutsRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            /*routes.MapNavigationRoute<HomeController>("Views", c => c.Index(), "Home")
                .AddChildRoute<HomeController>("Machines", c => c.Machines(), "Home")
                .AddChildRoute<HomeController>("Tasks", c => c.Tasks(), "Home");*/
            routes.MapNavigationRoute<AccountController>("Log out", c => c.LogOff());
        }
    }
}
