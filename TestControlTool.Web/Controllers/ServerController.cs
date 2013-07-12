using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestControlTool.Core.Exceptions;
using TestControlTool.Web.Models;

namespace TestControlTool.Web.Controllers
{
    public class ServerController : BootstrapMvcSample.Controllers.BootstrapBaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Servers", "Home");
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new ServerModel());
        }

        [HttpPost]
        public ActionResult Create(ServerModel model)
        {
            if (ModelState.IsValid)
            {
                model.Owner = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Id;

                try
                {
                    TestControlToolApplication.AccountController.AddVMServer(model.ToEntity());

                    Success("Your information was saved!");
                    return RedirectToAction("Index", "Server");
                }
                catch (AddExistingVMServerException)
                {
                    Error("There were some errors in your form. Such server is already presented");
                }
            }

            Error("There were some errors in your form.");

            return View(model);
        }

        public ActionResult Delete(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).VMServers.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such server");
            }

            TestControlToolApplication.AccountController.RemoveVMServer(id);

            Success("Your server was deleted");

            var machinesCount = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).VMServers.Count;

            if (machinesCount == 0)
            {
                Attention("You have deleted all servers!");
            }

            return RedirectToAction("Index", "Server");
        }

        public ActionResult Edit(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).VMServers.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such machine");
            }

            var model = TestControlToolApplication.AccountController.CachedServers.Single(x => x.Id == id).ToModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ServerModel model, Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).VMServers.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such machine");
            }

            if (ModelState.IsValid)
            {
                TestControlToolApplication.AccountController.EditVMServer(id, model.ToEntity());
                Success("Server '" + model.ServerName + "' was successfully updated!");
                return RedirectToAction("Index", "Server");
            }

            return View(model);
        }

        public ActionResult Details(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).VMServers.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such server");
            }

            var model = TestControlToolApplication.AccountController.CachedServers.Single(x => x.Id == id).ToModel();
            return View(model);
        }
    }
}
