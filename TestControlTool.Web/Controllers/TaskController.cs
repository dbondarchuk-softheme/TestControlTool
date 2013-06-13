using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Web;
using System.Web.Mvc;
using BootstrapMvcSample.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Exceptions;
using TestControlTool.Core.Models;
using TestControlTool.Management;
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
            return View(new TaskChildsModel()
                {
                    ChildTasks = new Collection<ChildTaskModel>(),
                    Task = new TaskModel()
                });
        }

        public ActionResult Edit(Guid id)
        {
            var account = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name);
            var task = account.Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException(id);
            }

            var childs = task.GetTaskChildsFromFile();

            var taskChildsModel = new TaskChildsModel
                {
                    ChildTasks = childs,
                    Task = TaskModel.FromITask(task)
                };

            return View(taskChildsModel);
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

        public ActionResult GetAvailableTestTasks()
        {
            var tests = TestControlToolApplication.AvailableTests;

            return View(tests);
        }

        public ActionResult DeployInstallJobModal()
        {
            var machines = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Machines;

            return PartialView(machines);
        }

        public ActionResult NewTestModal()
        {
            var tests = TestControlToolApplication.AvailableTests;

            return PartialView(tests);
        }

        public ActionResult TestForm(string testName)
        {
            var test = TestControlToolApplication.AvailableTests.Single(x => x.Name == testName);

            return PartialView(test);
        }

        public ActionResult ViewLogs(Guid id)
        {
            return View(id);
        }

        public JsonResult Run(Guid id)
        {
            var task = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            if (TestControlToolApplication.AccountController.Tasks.Single(x => x.Id == id).Status == TaskStatus.Running)
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
            var task = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Tasks.SingleOrDefault(x => x.Id == id);

            if (task == null)
            {
                throw new NoSuchTaskException("You don't have such task");
            }

            if (TestControlToolApplication.AccountController.Tasks.Single(x => x.Id == id).Status != TaskStatus.Running)
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

            var taskEnabled = TestControlToolApplication.AccountController.CachedTasks.Single(x => x.Id == id).IsEnabled;

            return Json(taskEnabled, JsonRequestBehavior.AllowGet);
        }
        
        public PartialViewResult GetLogs(Guid id)
        {
            try
            {
                using (var reader = new StreamReader(new FileStream(ConfigurationManager.AppSettings["LogsFolder"] + "\\" + id + ".log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var logs = reader.ReadToEnd();

                    var status = TestControlToolApplication.AccountController.Tasks.Single(x => x.Id == id).Status;

                    logs = "Task Status : " + status + "\n" + logs;

                    if (status == TaskStatus.Running)
                    {
                        logs += "\n<img src='" + Url.Content("~/Content/images/select2-spinner.gif") + "' />";
                    }

                    return PartialView("GetLogs", logs);
                }

            }
            catch (IOException)
            {
                return PartialView("GetLogs", string.Empty);
            }
        }

        public JsonResult GetStatuses()
        {
            var userId = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Id;
            var statuses = TestControlToolApplication.AccountController.Tasks.Where(x => x.Owner == userId).ToDictionary(x => x.Id.ToString(), y => y.Status.ToString());

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

        [HttpPost]
        public ActionResult UploadTestSuiteXml(Guid id, HttpPostedFileBase file)
        {
            var ownerId = TestControlToolApplication.AccountController.CachedAccounts.Single(x => x.Login == User.Identity.Name).Id;
            
            if (TestControlToolApplication.AccountController.CachedTasks.Any(x => x.Id == id && x.Owner != ownerId))
            {
                return Content("error1");
            }

            if (!file.FileName.EndsWith(".xml"))
            {
                return Content(/*"Wrong extension. Please, upload xml file"*/ "error2");
            }

            var fileName = id + "." + file.FileName;

            try
            {
                file.SaveAs(ConfigurationManager.AppSettings["TasksFolder"] + "\\" + fileName);

                var model = TestSuiteModel.GetFromXmlFile(fileName);

                return View("UploadedTestSuite", model);
            }
            catch
            {
                return Content("error3");
            }
        }
        
        [HttpPost]
        public JsonResult Create(string jsonModel)
        {
            var model = new TaskModel(JObject.Parse(jsonModel)["model"].ToString());
            model.Owner = TestControlToolApplication.AccountController.Accounts.Single(x => x.Login == User.Identity.Name).Id;

            if (!(TryValidateModel(model) && ValidationController.ValidateTaskModel(model, User.Identity.Name)))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            try
            {
                SaveChildsFromJson(jsonModel);

                var task = model.ToITask();

                TestControlToolApplication.AccountController.AddTask(task);

                Success("Task was succesfully created!");
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (AddExistingTaskException)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
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

            TestControlToolApplication.AccountController.EditTask(model.Id, model.ToITask());

            SaveChildsFromJson(jsonModel);

            Success("Task was successfully updated!");

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListItemModal(string typeName)
        {
            var type = TestControlToolApplication.TestPerformerTypes.SingleOrDefault(x => x.Name.ToLowerInvariant() == typeName.ToLowerInvariant());

            return View("ListItemModal", type);
        }

        private void SaveChildsFromJson(string jsonModel)
        {
            dynamic model = JObject.Parse(jsonModel);
            var jsonTasks = (JObject)JObject.Parse(model.tasks.Value);
            var jsonTests = (JObject)JObject.Parse(model.tests.Value);
            var jsonParameters = (JObject)JObject.Parse(model.parameters.Value);
            var taskModel = ((JObject)JObject.Parse(model.model.Value)).Properties().ToDictionary(x => x.Name, y => y.Value.ToString());

            var tasks = jsonTasks.Properties().ToDictionary(x => x.Name, y => JObject.Parse(y.Value.ToString()).Properties().ToDictionary(k => k.Name, v => v.Value.ToString()));

            var tests = jsonTests.Properties().ToDictionary(x => x.Name, y => ((JArray)JsonConvert.DeserializeObject(y.Value.ToString())).
                    Select(x => JObject.Parse(x.ToString()).Properties().ToDictionary(key => key.Name, value => value.Value.ToString())));

            var allParameters = jsonParameters.Properties().ToDictionary(x => x.Name, y => JObject.Parse(y.Value.ToString()).Properties().ToDictionary(k => k.Name,
                v => ((JArray)JsonConvert.DeserializeObject(v.Value.ToString())).Select(x => JObject.Parse(x.ToString()).Properties().ToDictionary(key => key.Name, value => value.Value.ToString()))));

            var childs = new List<ChildTaskModel>();

            foreach (var task in tasks)
            {
                var file = taskModel["id"] + "." + task.Value["id"] + ".xml";
                var taskType = TaskType.DeployInstall;

                if (task.Value["type"].ToUpperInvariant() == TaskType.DeployInstall.ToString().ToUpperInvariant())
                {
                    var deployInstallTaskModel = new DeployInstallTaskModel
                    {
                        Machines = TestControlToolApplication.AccountController.CachedMachines.Where(x => task.Value["machines"].Split(';').Contains(x.Id.ToString())).Select(x => x.Id)/* task.Value["machines"].Split(';').Select(x => new Guid(x))*/,
                        Type = (AutodeployJobType)Enum.Parse(typeof(AutodeployJobType), task.Value["deploytype"], true),
                        Name = task.Value["id"],
                        Version = task.Value["version"]
                    };

                    taskType = TaskType.DeployInstall;

                    deployInstallTaskModel.SaveToFile(file, User.Identity.Name);
                }
                else if (task.Value["type"].ToUpperInvariant() == TaskType.TestSuite.ToString().ToUpperInvariant())
                {
                    var testsList = new List<object>();

                    foreach (var testKey in tests.Single(x => x.Key == task.Value["id"]).Value)
                    {
                        var testType = TestControlToolApplication.AvailableTests.Single(x => x.Name == testKey["testtype"]);
                        var properties = testType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.CanRead).ToList();

                        var test = Activator.CreateInstance(testType);

                        var attributesToIgnore = new[] { "testtype", "style", "class", "id" };

                        foreach (var attribute in testKey.Where(x => !attributesToIgnore.Contains(x.Key.ToLowerInvariant())))
                        {
                            var property = properties.SingleOrDefault(x => x.Name.ToLowerInvariant() == attribute.Key.ToLowerInvariant());

                            if (property == null) continue;

                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(test, attribute.Value);
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                property.SetValue(test, attribute.Value.ConvertToEnum(property.PropertyType));
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                int value;
                                int.TryParse(attribute.Value, out value);

                                property.SetValue(test, value);
                            }
                        }

                        foreach (var parameters in allParameters.Where(x => x.Key.ToUpperInvariant() == testKey["id"].ToUpperInvariant()).Select(x => x.Value))
                        {
                            foreach (var parameter in parameters)
                            {
                                var property = properties.SingleOrDefault(x => x.Name.ToLowerInvariant() == parameter.Key.ToLowerInvariant());

                                if (property == null) continue;

                                if(!property.PropertyType.Name.Contains("List")) throw new ArgumentException("Wrong property type. It should be a List");

                                var list = Activator.CreateInstance(property.PropertyType);

                                var genericArgumentType = property.PropertyType.GetGenericArguments()[0];
                                var argumentProperties = genericArgumentType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.CanRead).ToList();

                                foreach (var item in parameter.Value)
                                {
                                    object argument;

                                    if (genericArgumentType == typeof(string))
                                    {
                                        argument = item["value"];
                                    }
                                    else if (genericArgumentType.IsEnum)
                                    {
                                        argument = item["value"].ConvertToEnum(genericArgumentType);
                                    }
                                    else if (genericArgumentType == typeof(int))
                                    {
                                        int value;
                                        int.TryParse(item["value"], out value);

                                        argument = value;
                                    }
                                    else
                                    {
                                        argument = Activator.CreateInstance(genericArgumentType);

                                        foreach (var attribute in item)
                                        {
                                            var argumentProperty = argumentProperties.SingleOrDefault(x => x.Name.ToLowerInvariant() == attribute.Key.ToLowerInvariant());

                                            if (argumentProperty == null) continue;

                                            if (argumentProperty.PropertyType == typeof (string))
                                            {
                                                argumentProperty.SetValue(argument, attribute.Value);
                                            }
                                            else if (argumentProperty.PropertyType.IsEnum)
                                            {
                                                argumentProperty.SetValue(argument, attribute.Value.ConvertToEnum(argumentProperty.PropertyType));
                                            }
                                        }
                                    }

                                    list.GetType().GetMethod("Add").Invoke(list, new[] {argument});
                                }

                                property.SetValue(test, list);
                            }
                        }

                        testsList.Add(test);
                    }

                    var testSuiteModel = new TestSuiteModel
                        {
                            Name = task.Value["id"],
                            Tests = testsList
                        };

                    taskType = TaskType.TestSuite;

                    testSuiteModel.SaveToFile(file);
                }

                childs.Add(new ChildTaskModel
                    {
                        File = file,
                        Name = task.Value["id"],
                        TaskType = taskType
                    });

            }

            childs.SaveTaskChildsToFile(new Guid(taskModel["id"]));
        }
    }
}
