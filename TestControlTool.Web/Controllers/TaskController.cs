using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using BootstrapMvcSample.Controllers;
using Ionic.Zip;
using Newtonsoft.Json.Linq;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Exceptions;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;
using TestControlTool.Web.Models;

namespace TestControlTool.Web.Controllers
{
    public class TaskController : BootstrapBaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Tasks", "Home");
        }

        public ActionResult Create()
        {
            return View(new TaskChildsModel
                {
                    ChildTasks = new Collection<ChildTaskModel>(),
                    Task = new TaskModel()
                });
        }

        public ActionResult Edit(Guid id)
        {
            var account = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name);
            var task = account.Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException(id);
            }

            if (task.Status == TaskStatus.Running)
            {
                Information("Sorry, but you can't edit task, while it is running.");

                return RedirectToAction("Index", "Task");
            }

            var childs = task.GetTaskChildsFromFile();

            var taskChildsModel = new TaskChildsModel
                {
                    ChildTasks = childs,
                    Task = task.ToModel()
                };

            return View(taskChildsModel);
        }

        public ActionResult Delete(Guid id)
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

            return RedirectToAction("Index", "Task");
        }

        /// <summary>
        /// Gets all available machines except some of them
        /// </summary>
        /// <param name="ids">List of already used machines</param>
        public ActionResult GetAllMachines(IEnumerable<Guid> ids)
        {
            var machines = TestControlToolApplication.AccountController.CachedAccounts.
                Single(x => x.Login == User.Identity.Name).Machines.Where(x => !ids.Contains(x.Id)).ToList();

            return PartialView(machines);
        }

        /// <summary>
        /// Gets all available machines 
        /// </summary>
        public ActionResult GetAllMachines()
        {
            var machines = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Machines;

            return PartialView(machines);
        }

        public ActionResult DeployInstallJobModal()
        {
            var machines = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Machines;

            return PartialView(machines);
        }

        public ActionResult NewTestModal(TaskType type = TaskType.UISuiteTrunk)
        {
            var tests = TestSuiteTypesHelper.GetAvailabaleTests(type);

            return PartialView(tests);
        }

        public ActionResult TestForm(string testName, TaskType type = TaskType.UISuiteTrunk)
        {
            var test = TestSuiteTypesHelper.GetAvailabaleTests(type).Single(x => x.FullName == testName);

            return PartialView(test);
        }

        public ActionResult ViewLogs(Guid id)
        {
            return View(id);
        }

        public JsonResult Run(Guid id)
        {
            var task = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            if (task.Status == TaskStatus.Running)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var taskWcfServiceClient = new TaskWcfServiceClient();
                taskWcfServiceClient.Open();
                taskWcfServiceClient.StartTask(id);
                taskWcfServiceClient.Close();
            }
            catch (Exception e)
            {
                return Json("unknown\n" + e.Message, JsonRequestBehavior.AllowGet);
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Stop(Guid id)
        {
            var task = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            if (task.Status != TaskStatus.Running)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var taskWcfServiceClient = new TaskWcfServiceClient();
                taskWcfServiceClient.Open();
                taskWcfServiceClient.StopTask(id);
                taskWcfServiceClient.Close();
            }
            catch (Exception)
            {
                return Json("unknown", JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ToogleEnabled(Guid id, bool enabled)
        {
            if (TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == id) == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            TestControlToolApplication.AccountController.SetTaskEnabled(id, enabled);

            var task = TestControlToolApplication.AccountController.CachedTasks.Single(x => x.Id == id).ToModel();

            return Json(new { enabled = task.IsEnabled, nextStart = task.NextRun }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetLogs(Guid id)
        {
            var task = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            var logs = "Task '" + task.Name + "', Status : " + task.Status + "<hr>\n";

            try
            {
                using (var reader = new StreamReader(new FileStream(ConfigurationManager.AppSettings["LogsFolder"] + "\\" + id + ".log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    logs += reader.ReadToEnd();

                    if (task.Status == TaskStatus.Running)
                    {
                        logs += "\n<img src='" + Url.Content("~/Content/images/select2-spinner.gif") + "' />";
                    }
                }

            }
            catch (IOException)
            {
                logs += "Sorry, but this task doesn't have any log";
            }

            return PartialView("GetLogs", logs);
        }

        public JsonResult GetStatuses()
        {
            var userId = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Id;
            var statuses = TestControlToolApplication.AccountController.Tasks.Where(x => x.Owner == userId).Select(x => x.ToModel()).ToDictionary(x => x.Id.ToString(), x => new { status = x.Status.ToString(), lastRun = x.LastRunExtended.ToString() });

            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetXmlFile(Guid taskId, string childTaskName)
        {
            var task = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == taskId);

            if (task == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            var childTask = task.GetTaskChildsFromFile().SingleOrDefault(x => x.Name == childTaskName);

            if (childTask == null)
            {
                throw new NoSuchTaskException("You haven't such child task");
            }

            if (childTask.TaskType == TaskType.DeployInstall)
            {
                var xmlParser = new XmlTaskParser(TestControlToolApplication.AccountController);
                xmlParser.ParseAutoDeployFiles(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + childTask.File, ".new2");

                var filesToZip = Core.Extensions.DeserializeFromFile<DeployInstallTaskContainer>(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + childTask.File)
                    .Files.Select(x => new FileInfo(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + x.Value + ".new2"));

                var fileName = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + task.Id + "." + childTask.Name + ".zip";

                using (var zip = new ZipFile())
                {
                    foreach (var file in filesToZip)
                    {
                        try
                        {
                            var zippedFile = zip.AddFile(file.FullName, "");
                            zippedFile.FileName = zippedFile.FileName.Remove(zippedFile.FileName.LastIndexOf(".new2", StringComparison.Ordinal));
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    zip.Save(fileName);

                    var contentDisposition = new System.Net.Mime.ContentDisposition
                    {
                        // for example foo.bak
                        FileName = fileName,

                        // always prompt the user for downloading, set to true if you want 
                        // the browser to try to show the file inline
                        Inline = false,
                    };

                    Response.AppendHeader("Content-Disposition", contentDisposition.ToString());
                    return File(fileName, "application/zip");
                }


            }
            else
            {
                var file = ConfigurationManager.AppSettings["TasksFolder"] + "\\" + childTask.File;

                var contentDisposition = new System.Net.Mime.ContentDisposition
                    {
                        // for example foo.bak
                        FileName = childTask.File,

                        // always prompt the user for downloading, set to true if you want 
                        // the browser to try to show the file inline
                        Inline = false,
                    };

                Response.AppendHeader("Content-Disposition", contentDisposition.ToString());
                return File(file, "text/xml");
            }
        }

        [HttpPost]
        public ActionResult UploadTestSuiteXml(Guid id, Guid machine, HttpPostedFileBase file, TaskType type = TaskType.UISuiteTrunk)
        {
            var ownerId = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Id;

            if (TestControlToolApplication.AccountController.CachedTasks.Any(x => x.Id == id && x.Owner != ownerId))
            {
                return Content("error1");
            }

            if (!file.FileName.EndsWith(".xml"))
            {
                return Content("error2");
            }

            var fileName = id + "." + file.FileName;

            try
            {
                file.SaveAs(ConfigurationManager.AppSettings["TasksFolder"] + "\\Temp\\" + fileName);

                var model = TestSuiteModel.GetFromXmlFile("Temp\\" + fileName, type, machine);

                if (!Regex.IsMatch(model.Name, @"^[a-zA-Z0-9_]+$"))
                {
                    return Content("error4");
                }

                return View("UploadedTestSuite", model);
            }
            catch (Exception e)
            {
                return Content("error3\n" + e.Message + (e.InnerException != null ? "." + e.InnerException.Message : ""));
            }
        }

        [HttpPost]
        public JsonResult Create(string jsonModel)
        {
            var model = new TaskModel(JObject.Parse(jsonModel)["model"].ToString());
            model.Owner = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Id;

            if (!(TryValidateModel(model) && ValidationController.ValidateTaskModel(model, User.Identity.Name)))
            {
                return Json("Please, correct your data", JsonRequestBehavior.AllowGet);
            }

            try
            {
                SaveChildsFromJson(jsonModel);

                var task = model.ToEntitiy();

                TestControlToolApplication.AccountController.AddTask(task);

                Success("Task was successfully created!");
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (AddExistingTaskException e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Edit(string jsonModel)
        {
            var model = new TaskModel(JObject.Parse(jsonModel)["model"].ToString());

            if (!(TryValidateModel(model) && ValidationController.ValidateTaskModel(model, User.Identity.Name)))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            try
            {
                SaveChildsFromJson(jsonModel);

                TestControlToolApplication.AccountController.EditTask(model.Id, model.ToEntitiy());
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
            
            Success("Task was successfully updated!");

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListItemModal(string typeName, string parentProperty = "", TaskType suiteType = TaskType.UISuiteTrunk)
        {
            var standartType = Type.GetType(typeName);

            var type = standartType ?? (TestSuiteTypesHelper.GetScriptsTypes(suiteType).SingleOrDefault(x => x.FullName == typeName));

            return View("ListItemModal", new Pair<Type, string>(type, parentProperty));
        }

        private void SaveChildsFromJson(string jsonModel)
        {
            dynamic model = JObject.Parse(jsonModel);

            var jsonTasks = (JArray)model.tasks;
            var taskModel = ((JObject)model.model).Properties().ToDictionary(x => x.Name, y => y.Value.ToString());

            var childs = new List<ChildTaskModel>();

            foreach (var task in jsonTasks.Select(jsonTask => JObject.Parse(jsonTask.ToString()).Properties().ToDictionary(x => x.Name, y => y.Value)))
            {
                var file = taskModel["id"] + "." + task["id"].ToObject<string>() + ".xml";

                var taskType = (TaskType)(Enum.Parse(typeof(TaskType), task["type"].ToObject<string>(), true));

                if (taskType == TaskType.DeployInstall)
                {
                    var deployInstallTaskModel = new DeployInstallTaskModel
                        {
                            Machines = TestControlToolApplication.AccountController.CachedMachines.Where(
                                    x => task["machines"].ToObject<string>().Split(',').Contains(x.Id.ToString())).Select(x => x.Id),
                            Type = (DeployInstallType)Enum.Parse(typeof (DeployInstallType), task["deploytype"].ToObject<string>(), true),
                            Name = task["id"].ToObject<string>(),
                            Version = task["version"].ToObject<string>(),
                            Build = task["build"].ToObject<string>()
                        };

                    taskType = TaskType.DeployInstall;

                    deployInstallTaskModel.SaveToFile(file, User.Identity.Name);
                }
                else if (taskType == TaskType.UISuiteTrunk || taskType == TaskType.UISuiteRelease
                   || taskType == TaskType.BackendSuiteTrunk || taskType == TaskType.BackendSuiteRelease)
                {
                    var testsList = new List<object>();

                    foreach (var test in JArray.Parse(task["json"].ToString()))
                    {
                        var availableTypes = TestSuiteTypesHelper.GetScriptsTypes(taskType).ToList();

                        var testType = availableTypes.Single(x => x.FullName == test["type"].ToObject<string>());

                        var stream = new MemoryStream(Encoding.Unicode.GetBytes(test["object"].ToString()));
                        var serializer = new DataContractJsonSerializer(testType, availableTypes);

                        testsList.Add(serializer.ReadObject(stream));
                    }
                    
                    var testSuiteModel = new TestSuiteModel
                        {
                            Name = task["id"].ToObject<string>(),
                            Tests = testsList,
                            Type = taskType,
                            Machine = new Guid(task["machine"].ToObject<string>())
                        };

                    testSuiteModel.SaveToFile(file);
                }

                childs.Add(new ChildTaskModel
                    {
                        File = file,
                        Name = taskModel["id"],
                        TaskType = taskType
                    });
            }

            childs.SaveTaskChildsToFile(new Guid(taskModel["id"]));
        }
    }
}
