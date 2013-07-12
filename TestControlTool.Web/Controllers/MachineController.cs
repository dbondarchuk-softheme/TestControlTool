using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TestControlTool.Core.Exceptions;
using TestControlTool.Core.Implementations;
using TestControlTool.Web.Models;

namespace TestControlTool.Web.Controllers
{
    public class MachineController : BootstrapMvcSample.Controllers.BootstrapBaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Machines", "Home");
        }

        [HttpPost]
        public ActionResult Create(MachineModel model)
        {
            if (ModelState.IsValid)
            {
                model.Owner = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Id;

                try
                {
                    TestControlToolApplication.AccountController.AddMachine(model.ToEntity());

                    Success("Your information was saved!");
                    return RedirectToAction("Index", "Machine");
                }
                catch (AddExistingMachineException)
                {
                    Error("There were some errors in your form. Such machine is already presented");
                }
            }

            Error("There were some errors in your form.");

            return View(model);
        }

        public ActionResult Create(VMServerType destinationType = VMServerType.VCenter)
        {
            var model = destinationType == VMServerType.VCenter
                            ? new VCenterMachineModel()
                            : ((MachineModel) (new HyperVMachineModel()));

            model.DeployOn = GetServers(destinationType).ToSelectList(x => x.ServerName, x => x.Id.ToString());

            if (!model.DeployOn.Any())
            {
                Error("Sorry, but you don't have any suitable " + destinationType + " VM server. Please, create it");

                return RedirectToAction("Index", "Machine");
            }

            return View(model);
        }

        public ActionResult Delete(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such machine");
            }

            TestControlToolApplication.AccountController.RemoveMachine(id);

            Success("Your machine was deleted");

            var machinesCount = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Count;

            if (machinesCount == 0)
            {
                Attention("You have deleted all machines!");
            }

            return RedirectToAction("Index", "Machine");
        }

        public ActionResult Edit(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such machine");
            }

            var model = TestControlToolApplication.AccountController.CachedMachines.Single(x => x.Id == id).ToModel();
            
            model.DeployOn = GetServers(model is VCenterMachineModel ? VMServerType.VCenter : VMServerType.HyperV).ToSelectList(x => x.ServerName, x => x.Id.ToString(), x => x.Id == id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MachineModel model, Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such machine");
            }
            
            if (ModelState.IsValid)
            {
                TestControlToolApplication.AccountController.EditMachine(id, model.ToEntity());
                Success("Machine '" + model.Name + "' was successfully updated!");
                return RedirectToAction("Index", "Machine");
            }

            return View(model);
        }

        public ActionResult Details(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You don't have such machine");
            }

            var model = TestControlToolApplication.AccountController.CachedMachines.Single(x => x.Id == id).ToModel();
            model.DeployOn = GetServers(model is VCenterMachineModel ? VMServerType.VCenter : VMServerType.HyperV).ToSelectList(x => x.ServerName, x => x.Id.ToString(), x => x.Id == id);

            return View(model);
        }

        private IEnumerable<ServerModel> GetServers(VMServerType type)
        {
            return TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).VMServers.Where(x => x.Type == type).Select(x => x.ToModel());
        }
    }
}
