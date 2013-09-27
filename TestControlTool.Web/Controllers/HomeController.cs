using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BootstrapMvcSample.Controllers;
using TestControlTool.Core.Implementations;
using TestControlTool.Web.Models;

namespace TestControlTool.Web.Controllers
{
    public class HomeController : BootstrapBaseController
    {
        public ActionResult Index()
        {
            return Tasks();
        }

        public ActionResult Tasks()
        {
            var tasks = TestControlToolApplication.AccountController.Accounts.First(x => x.Login == User.Identity.Name).Tasks.Select(x => x.ToModel()).OrderBy(x => x.Name).ToList();

            if (!tasks.Any())
            {
                Information("You don't have any task. Create a new one to start");
            }

            return View("Tasks", tasks);
        }

        public ActionResult Machines()
        {
            var machines = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Select(x => x.ToModel()).ToList();
            
            if (!machines.Any())
            {
                Information("You don't have any machine. Create a new one to start");
            }

            return View("Machines", machines);
        }

        public ActionResult Servers()
        {
            var servers = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).VMServers.Select(x => x.ToModel());

            if (!servers.Any())
            {
                Information("You don't have any server. Create a new one to start");
            }

            return View("Servers", servers);
        }

        public ActionResult Help()
        {
            return View();
        }

        public FileResult ScriptsDownload(VMServerType type = VMServerType.VCenter)
        {
            var fileName = "";

            switch (type)
            {
                case VMServerType.VCenter:
                    fileName = "ConfigureVCenterMachine.ps1";
                    break;
                   
                case VMServerType.HyperV:
                    fileName = "ConfigureChallengerVM.ps1";
                    break;

                default:
                    throw new ArgumentException("Wrong type");
            }

            var bytes = Encoding.Unicode.GetBytes(System.IO.File.ReadAllText(Server.MapPath("~/App_Data/" + fileName)));

            return File(bytes, "text/plain", fileName);
        }
    }
}
