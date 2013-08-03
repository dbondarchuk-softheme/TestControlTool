using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;

namespace TestControlTool.TaskService
{
    public static class MachineConfigurationService
    {
        private static readonly IAccountController AccountController = CastleResolver.Resolve<IAccountController>();

        public static bool ConfigureMachine(Guid id)
        {
            try
            {
                var machine = AccountController.Machines.Single(x => x.Id == id);

                var task = new MachineConfigurationTask
                    {
                        Machine = machine
                    };

                Task.Factory.StartNew(task.Run);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int GetStatusOfConfiguring(Guid id)
        {
            try
            {
                var status = MachineConfigurationTask.GetStatus(id);

                return status >=0 && status <= 100 ? status : -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
