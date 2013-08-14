using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;
using TestControlTool.TaskService;

namespace TestControlTools.Tests
{
    [TestFixture]
    public class AdditionalTests
    {
        private  IAccountController _accountController;

        [SetUp]
        private void SetUp()
        {
            File.Copy("D:\\accounts.sdf", "accounts2.sdf", true);

            _accountController = new SqlCEAccountController();
        }

        [Test]
        public void VCenterAutodeployLoggingTest()
        {
            var task = _accountController.CachedTasks.Single(x => x.Id == new Guid("c29ac581-c291-4c60-b179-40392a8cdbc2"));

            task.Start(new CancellationToken()).Wait();
        }

        [Test]
        public void MachineConfiguringTest()
        {
            var machine = new MachineConfigurationModel()
                {
                    AutoLogonPassword = "123asdQ",
                    AutoLogonUserName = "administrator",
                    ComputerName = "E1-2K12S-S12",
                    Id = Guid.NewGuid(),
                    OwnerUserName = "dmitriy.bondarchuk@softheme.com",
                    DefaultGateway = "10.35.0.1",
                    Dns1 = "10.35.20.10",
                    Dns2 = "10.35.20.11",
                    IPAddress = "10.35.7.50",
                    MachineType = VMServerType.HyperV,
                    SharedFolderPath = @"C:\Share3",
                    SubnetMask = "255.255.0.0",
                    TimeZoneName = "FLE Standard Time"
                };

            var task = new MachineConfigurationTask()
                {
                    MachineConfigurationModel = machine
                };

            MachineConfigurationService.ConfigureMachine(machine);
        }
    }
}
