using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Moq;
using NUnit.Framework;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;

namespace TestControlTools.Tests
{
    [TestFixture]
    public class XmlTaskParserTests
    {
        private XmlTaskParser _parser;
        private Mock<IAccountController> _accountController;
        private Mock<ILogger> _logger;
        
        public Guid Task { get; private set; }

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger>();

            _accountController = new Mock<IAccountController>();

            var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Login = "1"
                };
            
            var machines = new List<IMachine>();
            var tasks = new List<IScheduleTask>();

            for (var i = 0; i < 3; i++)
            {
                machines.Add(new Machine
                    {
                        Id = Guid.NewGuid(),
                        Owner = account.Id,
                        Name = "New",
                        Address = "10.35.174.80",
                        UserName = "Administrator",
                        Password = "123asdQ"
                    });

                tasks.Add(new ScheduleTask
                    {
                        Id = Guid.NewGuid(),
                        Owner = account.Id
                    });
            }

            account.Machines = new List<IMachine>(machines);
            account.Tasks = new List<IScheduleTask>(tasks);

            _accountController.SetupGet(x => x.CachedMachines).Returns(new List<IMachine>(machines));
            _accountController.SetupGet(x => x.CachedTasks).Returns(new List<IScheduleTask>(tasks));

            _parser = new XmlTaskParser(_accountController.Object);

            Task = _accountController.Object.CachedTasks.First().Id;
            
            //CreateMachinesXml();
            GenerateMainXml();
            GenerateAutodeploySource();
            GenerateTestsSource();
        }

        public void CreateMachinesXml()
        {
            var machines = _accountController.Object.CachedMachines.Select(x => x.Id);

            var collection = new Collection<Guid>(machines.ToList());

            using (var machinesFile = File.Open("Tasks\\" + Task + ".Machines.xml", FileMode.Create))
            {
                var machinesListDeserializer = new XmlSerializer(typeof(Collection<Guid>));

                machinesListDeserializer.Serialize(machinesFile, collection);
            }
        }

        public void GenerateMainXml()
        {
            var childTasks = new Collection<ChildTaskModel>
                {
                    /*new ChildTaskModel
                        {
                            TaskType = TaskType.DeployInstall,
                            File = "AutodeploySource2.xml",
                            Name = "AutoDeploy"
                        },*/
                    new ChildTaskModel
                            {
                                TaskType = TaskType.UISuiteTrunk,
                                File = "Tests2.xml",
                                Name = "Test"
                            }
                };

            using (var machinesFile = File.Open("Tasks\\" + Task + ".xml", FileMode.Create))
            {
                var machinesListDeserializer = new XmlSerializer(typeof(Collection<ChildTaskModel>));

                machinesListDeserializer.Serialize(machinesFile, childTasks);
            }
        }

        public void GenerateAutodeploySource()
        {
            File.Copy("Tasks\\AutodeploySource.xml", "Tasks\\AutodeploySource2.xml", true);

            File.WriteAllText("Tasks\\AutodeploySource2.xml", File.ReadAllText("Tasks\\AutodeploySource2.xml", new UnicodeEncoding())
                .Replace("ID",_accountController.Object.CachedMachines.First().Id.ToString()), new UnicodeEncoding());
        }

        public void GenerateTestsSource()
        {
            File.Copy("Tasks\\Tests.xml", "Tasks\\Tests2.xml", true);

            File.WriteAllText("Tasks\\Tests2.xml", File.ReadAllText("Tasks\\Tests2.xml", new UnicodeEncoding())
                .Replace("ID", _accountController.Object.CachedMachines.First().Id.ToString()), new UnicodeEncoding());
        }

        [Test]
        public void Parse()
        {
            Assert.AreEqual(1, _parser.Parse(Task, _logger.Object).Count());
        }

        [Test]
        public void TaskStart()
        {
            File.Copy("accounts.sdf", "accounts2.sdf", true);
            ((ScheduleTask)_accountController.Object.CachedTasks.First()).Start(new XmlTaskParser(_accountController.Object), new CancellationToken()).Wait();
        }

        [TearDown]
        public void Clean()
        {
            File.Delete("Tasks\\" + Task + ".Machines.xml");
            File.Delete("Tasks\\" + Task + ".xml");
            File.Delete("Tasks\\AutodeploySource2.xml");
            File.Delete("Tasks\\Tests2.xml");
        }
    }
}
