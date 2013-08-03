using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using BootstrapSupport;
using TestControlTool.Core.Models;

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
            AddMessage(message, Alerts.ATTENTION);
        }

        public void Success(string message)
        {
            AddMessage(message, Alerts.SUCCESS);
        }

        public void Information(string message)
        {
            AddMessage(message, Alerts.INFORMATION);
        }

        public void Error(string message)
        {
            AddMessage(message, Alerts.ERROR);
        }

        private void AddMessage(string message, string type)
        {
            if (!TempData.ContainsKey("Alerts"))
            {
                var data = new List<Pair<string, string>>
                    {
                        new Pair<string, string>(type, message)
                    };

                TempData.Add("Alerts", data);
            }
            else
            {
                var data = TempData["Alerts"] as List<Pair<string, string>> ?? new List<Pair<string, string>>();

                data.Add(new Pair<string, string>(type, message));

                TempData["Alerts"] = data;
            }
        }
    }
}
