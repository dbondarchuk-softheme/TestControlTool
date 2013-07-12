using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestControlTool.Core.Implementations;
using TestControlTool.Web.Models;

namespace TestControlTool.Web.Controllers
{
    [AllowAnonymous]
    public class ValidationController : Controller
    {
        public JsonResult ValidateFrequency(TaskModel task)
        {
            return Json(/*String.IsNullOrEmpty(task.StartTime) || IsTaskStartTimeValid(task.StartTime)*/ true, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult CheckUser(string UserName)
        {
            if (String.IsNullOrEmpty(UserName)) return Json(true, JsonRequestBehavior.AllowGet);

            var result = !TestControlToolApplication.AccountController.CachedAccounts.Any(x => x.Login == UserName);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateUniqueTaskName(TaskModel task)
        {
            return Json(String.IsNullOrEmpty(task.Name) || IsTaskNameUnique(task.Name, task.Id, User.Identity.Name), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateUniqueMachineName(string name, Guid id)
        {
            if (String.IsNullOrEmpty(name)) return Json(true, JsonRequestBehavior.AllowGet);
            
            var result = !TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).
                     Machines.Any(x => x.Name == name.Trim() && x.Id != id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateUniqueMachineAddress(string address, Guid id)
        {
            if (String.IsNullOrEmpty(address)) return Json(true, JsonRequestBehavior.AllowGet);

            var result = !TestControlToolApplication.AccountController.CachedMachines.Any(x => x.Address == address.Trim() && x.Id != id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateUniqueMachineHost(string host, Guid id)
        {
            if (String.IsNullOrEmpty(host)) return Json(true, JsonRequestBehavior.AllowGet);

            var result = !TestControlToolApplication.AccountController.CachedMachines.Any(x => x.Host == host.Trim() && x.Id != id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateUniqueMachineTemplateVMName(string templateVMName, Guid id)
        {
            if (String.IsNullOrEmpty(templateVMName)) return Json(true, JsonRequestBehavior.AllowGet);

            //var result = !TestControlToolApplication.AccountController.CachedMachines.Any(x => x.TemplateVMName == templateVMName.Trim() && x.Id != id);

            return Json(/*result*/ true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateUniqueServerAddress(string serverName, Guid id)
        {
            if (String.IsNullOrEmpty(serverName)) return Json(true, JsonRequestBehavior.AllowGet);

            var result = !TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).VMServers.Any(x => x.ServerName == serverName.Trim() && x.Id != id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public static bool IsTaskStartTimeValid(DateTime startTime)
        {
            try
            {
                var taskModel = new ScheduleTask()
                {
                    StartTime = startTime
                };

                taskModel.IsTimeToStart(DateTime.Now);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool IsTaskNameUnique(string name, Guid id, string userName)
        {
            return !TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == userName).
                     Tasks.Any(x => x.Name == name && x.Id != id);
        }

        public static bool ValidateTaskModel(TaskModel model, string userName)
        {
            return IsTaskStartTimeValid(model.StartTime) && IsTaskNameUnique(model.Name, model.Id, userName);
        }
    }
}
