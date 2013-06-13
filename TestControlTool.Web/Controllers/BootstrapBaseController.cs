using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using BootstrapSupport;

namespace BootstrapMvcSample.Controllers
{
    [Authorize]
    public class BootstrapBaseController: Controller
    {
        public ActionResult GetMachinesListSidebar()
        {
            var machines =TestControlTool.Web.TestControlToolApplication.AccountController.CachedAccounts.Single(
                    x => x.Login == User.Identity.Name).Machines.ToList();

            return View(machines);
        }

        public ActionResult GetTasksListSidebar()
        {
            var tasks = TestControlTool.Web.TestControlToolApplication.AccountController.CachedAccounts.Single(
                    x => x.Login == User.Identity.Name).Tasks.ToList();

            return View(tasks);
        }

        public void Attention(string message)
        {
            TempData.Add(message, Alerts.ATTENTION);
        }

        public void Success(string message)
        {
            TempData.Add(Alerts.SUCCESS, message);
        }

        public void Information(string message)
        {
            TempData.Add(Alerts.INFORMATION, message);
        }

        public void Error(string message)
        {
            TempData.Add(Alerts.ERROR, message);
        }
    }
}
