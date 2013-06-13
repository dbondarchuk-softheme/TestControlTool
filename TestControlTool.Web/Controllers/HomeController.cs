using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using BootstrapMvcSample.Controllers;
using Models;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Exceptions;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;
using TestControlTool.Management;
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
            var tasks = TestControlToolApplication.AccountController.Accounts.First(x => x.Login == User.Identity.Name).Tasks.Select(TaskModel.FromITask).ToList();

            if (!tasks.Any())
            {
                Information("You don't have any task. Create a new one to start");
            }

            return View("Tasks", tasks);
        }

        public ActionResult Machines()
        {
            var machines = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Select(MachineModel.FromIMachine);

            if (!machines.Any())
            {
                Information("You don't have any machine. Create a new one to start");
            }

            return View("Machines", machines);
        }

        [HttpPost]
        public ActionResult CreateMachine(MachineModel model)
        {
            if (ModelState.IsValid)
            {
                model.Owner = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Id;

                try
                {
                    TestControlToolApplication.AccountController.AddMachine(model);

                    Success("Your information was saved!");
                    return RedirectToAction("Machines");
                }
                catch(AddExistingMachineException)
                {
                    Error("There were some errors in your form. Such machine is already presented");
                }
            }

            Error("There were some errors in your form.");

            return View(model);
        }

        public ActionResult CreateMachine()
        {
            return View(new MachineModel());
        }

        public ActionResult DeleteMachine(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to remove this machine");
            }

            TestControlToolApplication.AccountController.RemoveMachine(id);

            Success("Your machine was deleted");

            var machinesCount = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Count;

            if(machinesCount == 0)
            {
                Attention("You have deleted all machine! Create a new one to continue.");
            }

            return RedirectToAction("Machines");
        }

        public ActionResult EditMachine(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to edit this machine");
            }

            var model = MachineModel.FromIMachine(TestControlToolApplication.AccountController.CachedMachines.Single(x => x.Id == id));
            return View(model);
        }
        
        [HttpPost]        
        public ActionResult EditMachine(MachineModel model, Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to edit this machine");
            }

            if(ModelState.IsValid)
            {
                TestControlToolApplication.AccountController.EditMachine(id, model);
                Success("Machine '" + model.Name + "' was successfully updated!");
                return RedirectToAction("Machines");
            }

            return View(model);
        }

		public ActionResult MachineDetails(Guid id)
		{
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Machines.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to view details of this machine");
            }

		    var model = MachineModel.FromIMachine(TestControlToolApplication.AccountController.CachedMachines.Single(x => x.Id == id));
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateTask(TaskModel model)
        {
            if (ModelState.IsValid)
            {
                model.Owner = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Id;

                try
                {
                    var task = model.ToITask();

                    TestControlToolApplication.AccountController.AddTask(task);
                    task.CreateTaskFiles();

                    Success("Your information was saved!");
                    return RedirectToAction("EditTaskChilds", "Task", new {id = task.Id});
                }
                catch (AddExistingTaskException)
                {
                    Error("There were some errors in your form. Such task is already presented");
                }
            }
            else
            {
                Error("There were some errors in your form.");
            }


            return View(model);
        }

        public ActionResult CreateTask()
        {
            return RedirectToAction("Create", "Task");
        }

        public ActionResult DeleteTask(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Tasks.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to remove this task");
            }

            TestControlToolApplication.AccountController.RemoveTask(id);

            Success("Your task was deleted");

            var machinesCount = TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Tasks.Count;

            if (machinesCount == 0)
            {
                Attention("You have deleted all tasks! Create a new one to continue.");
            }

            return RedirectToAction("Tasks");
        }

        public ActionResult EditTask(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Tasks.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to edit this task");
            }

            var model = TaskModel.FromITask(TestControlToolApplication.AccountController.CachedTasks.Single(x => x.Id == id));
            return RedirectToAction("Edit", "Task", model);
        }

        [HttpPost]
        public ActionResult EditTask(TaskModel model, Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Tasks.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to edit this task");
            }

            if (ModelState.IsValid)
            {
                TestControlToolApplication.AccountController.EditTask(id, model.ToITask());

                Success("Task was successfully updated!");

                //return RedirectToAction("Tasks");
                return RedirectToAction("Edit", "Task", new { id = id} );
            }

            return View(model);
        }

        public ActionResult TaskDetails(Guid id)
        {
            if (!TestControlToolApplication.AccountController.CachedAccounts.First(x => x.Login == User.Identity.Name).Tasks.Any(x => x.Id == id))
            {
                throw new UnauthorizedAccessException("You haven't needed rights to view details of this task");
            }

            var model = TaskModel.FromITask(TestControlToolApplication.AccountController.CachedTasks.Single(x => x.Id == id));
            return View(model);
        }
    }
}
