using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Exceptions;
using TestControlTool.Core.Implementations;

namespace TestControlTools.Tests
{
    [TestFixture]
    public class SqlCEAccountControllerTests
    {
        private SqlCEAccountController _controller;

        [SetUp]
        public void SetUp()
        {
            File.Copy("accounts.sdf", "accounts2.sdf", true);
            _controller = new SqlCEAccountController();
        }

        [Test]
        public void ShowAllAccounts()
        {
            foreach (var account in _controller.Accounts)
            {
                Console.WriteLine(account.Id + " " + account.Login + " " + account.PasswordHash + " " + (new Md5PasswordHash()).GetHash(account.PasswordHash));

                foreach (var machine in account.Machines)
                {
                    Console.WriteLine(machine.Name);
                }
            }

            foreach (var machine in _controller.Machines)
            {
                Console.WriteLine(machine.Id + " " + machine.Name + " " + machine.Owner + " " + machine.Address);
            }
        }

        [Test]
        public void AddAccountTest()
        {
            var account = new Account
                {
                    Id = Guid.NewGuid(),
                    Login = "Login",
                    PasswordHash = "Hash",
                    Machines = new Collection<IMachine>(),
                    Tasks = new Collection<IScheduleTask>()
                };

            _controller.AddAccount(account);

            Assert.True(_controller.CachedAccounts.Any(x => x.Id == account.Id));
        }

        [Test]
        [ExpectedException(typeof(AddExistingAccountException))]
        public void AddExistingAccountTest()
        {
            var account = new Account
                {
                    Id = Guid.NewGuid(),
                    Login = "Login",
                    PasswordHash = "Hash",
                    Machines = new Collection<IMachine>(),
                    Tasks = new Collection<IScheduleTask>()
                };

            _controller.AddAccount(account);
            _controller.AddAccount(account);
        }

        [Test]
        public void AddMachineTest()
        {
            var machine = new Machine
                {
                    Address = "10.35.174.80",
                    Name = "Name",
                    Password = "123asdQ",
                    UserName = "admin",
                    Type = MachineType.Core,
                    Host = "a",
                    TemplateInventoryPath = "v",
                    TemplateVMName = "a",
                    VirtualMachineVMName = "s",
                    VirtualMachineDatastore = "a",
                    VirtualMachineInventoryPath = "a",
                    VirtualMachineResourcePool = "a",
                    Id = Guid.NewGuid(),
                    Owner = _controller.Accounts.First().Id
                };

            _controller.AddMachine(machine);

            Assert.True(_controller.CachedMachines.Any(x => x.Id == machine.Id && x.Owner == machine.Owner && x.Name == machine.Name));
        }

        [Test]
        [ExpectedException(typeof(AddExistingMachineException))]
        public void AddExistingMachineTest()
        {
            var machine = new Machine
            {
                Address = "10.35.174.80",
                Type = MachineType.Core,
                Host = "a",
                TemplateInventoryPath = "v",
                TemplateVMName = "a",
                VirtualMachineVMName = "s",
                VirtualMachineDatastore = "a",
                VirtualMachineInventoryPath = "a",
                VirtualMachineResourcePool = "a",
                Name = "Name",
                Password = "123asdQ",
                UserName = "admin",
                Id = Guid.NewGuid(),
                Owner = _controller.Accounts.First().Id
            };

            _controller.AddMachine(machine);
            _controller.AddMachine(machine);
        }

        [Test]
        [ExpectedException(typeof(NoSuchAccountException))]
        public void AddMachineWrongAccountTest()
        {
            var machine = new Machine
            {
                Address = "10.35.174.80",
                Type = MachineType.Core,
                Host = "a",
                TemplateInventoryPath = "v",
                TemplateVMName = "a",
                VirtualMachineVMName = "s",
                VirtualMachineDatastore = "a",
                VirtualMachineInventoryPath = "a",
                VirtualMachineResourcePool = "a",
                Name = "Name",
                Password = "123asdQ",
                UserName = "admin",
                Id = Guid.NewGuid(),
                Owner = Guid.NewGuid()
            };

            _controller.AddMachine(machine);
        }

        [Test]
        public void EditMachineTest()
        {
            var dbMachine = _controller.Machines.First();

            var machine = new Machine
            {
                Address = "10.35.174.80",
                Type = MachineType.Core,
                Host = "a",
                TemplateInventoryPath = "v",
                TemplateVMName = "a",
                VirtualMachineVMName = "s",
                VirtualMachineDatastore = "a",
                VirtualMachineInventoryPath = "a",
                VirtualMachineResourcePool = "a",
                Name = "Name",
                Password = "123asdQ",
                UserName = "admin",
                Id = Guid.NewGuid(),
                Owner = Guid.NewGuid()
            };

            _controller.EditMachine(dbMachine.Id, machine);

            Assert.True(_controller.Machines.Any(x => x.Id == dbMachine.Id && x.Owner == dbMachine.Owner && x.Name == machine.Name && x.Address == machine.Address));
        }

        [Test]
        [ExpectedException(typeof(NoSuchMachineException))]
        public void EditNotExistingMachineTest()
        {
            var machine = new Machine
            {
                Address = "10.35.174.80",
                Type = MachineType.Core,
                Host = "a",
                TemplateInventoryPath = "v",
                TemplateVMName = "a",
                VirtualMachineVMName = "s",
                VirtualMachineDatastore = "a",
                VirtualMachineInventoryPath = "a",
                VirtualMachineResourcePool = "a",
                Name = "Name",
                Password = "123asdQ",
                UserName = "admin",
                Id = Guid.NewGuid(),
                Owner = Guid.NewGuid()
            };

            _controller.EditMachine(Guid.NewGuid(), machine);
        }

        [Test]
        public void RemoveMachineTest()
        {
            var id = _controller.Machines.First().Id;

            _controller.RemoveMachine(id);

            Assert.False(_controller.Machines.Any(x => x.Id == id));
        }

        [Test]
        public void AddTaskTest()
        {
            var task = new ScheduleTask()
                {
                    Id = Guid.NewGuid(),
                    Owner = _controller.Accounts.First().Id,
                    IsEnabled = true,
                    StartTime = new DateTime(1934, 3, 5, 3, 4, 5),
                    EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                    LastRun = DateTime.Now,
                    Frequency = "2 3 * *",
                    Name = "Name"
                };

            _controller.AddTask(task);

            Assert.True(_controller.CachedTasks.Any(x => x.Id == task.Id && x.Owner == task.Owner && x.StartTime == task.StartTime));
        }

        [Test]
        [ExpectedException(typeof(AddExistingTaskException))]
        public void AddExistingTaskTest()
        {
            var task = new ScheduleTask()
            {
                Id = Guid.NewGuid(),
                Owner = _controller.Accounts.First().Id,
                IsEnabled = true,
                StartTime = new DateTime(1934, 3, 5, 3, 4, 5),
                EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                LastRun = DateTime.Now,
                Frequency = "2 3 * *",
                Name = "Task"
            };

            _controller.AddTask(task);
            _controller.AddTask(task);
        }

        [Test]
        [ExpectedException(typeof(NoSuchAccountException))]
        public void AddTaskWrongAccountTest()
        {
            var task = new ScheduleTask()
            {
                Id = Guid.NewGuid(),
                Owner = Guid.NewGuid(),
                IsEnabled = true,
                StartTime = new DateTime(1934, 3, 5, 3, 4, 5),
                EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                Frequency = "2 3 * *"
            };

            _controller.AddTask(task);
        }

        [Test]
        public void EditTaskTest()
        {
            var dbTask = _controller.Tasks.First();

            var task = new ScheduleTask
            {
                Id = Guid.Empty,
                Owner = Guid.NewGuid(),
                IsEnabled = true,
                StartTime = new DateTime(1934, 3, 5, 3, 4, 5),
                EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                LastRun = DateTime.Now,
                Frequency = "2 3 * *",
                Name = "Name"
            };

            _controller.EditTask(dbTask.Id, task);

            Assert.True(_controller.CachedTasks.Any(x => x.Id == dbTask.Id && x.Owner == dbTask.Owner && x.StartTime == task.StartTime));
        }

        [Test]
        [ExpectedException(typeof(NoSuchTaskException))]
        public void EditNotExistingTaskTest()
        {
            var task = new ScheduleTask()
            {
                Id = Guid.NewGuid(),
                Owner = _controller.Accounts.First().Id,
                IsEnabled = true,
                StartTime = new DateTime(1934, 3, 5, 3, 4, 5),
                EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                Frequency = "2 3 * *"
            };

            _controller.EditTask(Guid.NewGuid(), task);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddTaskTimeToStartExceptionStartTimeBiggerThanEndTimeTest()
        {
            var task = new ScheduleTask()
            {
                Id = Guid.NewGuid(),
                Owner = _controller.Accounts.First().Id,
                IsEnabled = true,
                StartTime = new DateTime(2034, 3, 5, 3, 4, 5),
                EndTime = new DateTime(2015, 3, 5, 3, 4, 5),
                LastRun = DateTime.Now,
                Frequency = "2 3 * *"
            };

            _controller.AddTask(task);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EditTaskTimeToStartExceptionStartTimeBiggerThanEndTimeTest()
        {
            var dbTask = _controller.Tasks.First();

            var task = new ScheduleTask
                {
                    StartTime = new DateTime(2034, 3, 5, 3, 4, 5),
                    EndTime = new DateTime(2015, 3, 5, 3, 4, 5)
                };

            _controller.EditTask(dbTask.Id, task);
        }

        [Test]
        public void RemoveTaskTest()
        {
            var id = _controller.Tasks.First().Id;

            _controller.RemoveTask(id);

            Assert.False(_controller.Tasks.Any(x => x.Id == id));
        }
    }
}
