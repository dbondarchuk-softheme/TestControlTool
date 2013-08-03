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
    }
}
