using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TaskStatus = TestControlTool.Core.Contracts.TaskStatus;

namespace TestControlTool.TaskService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class TaskWcfService : ITaskWcfService
    {
        public bool StartTask(Guid id)
        {
            return TaskService.RunTask(id);
        }

        public bool StopTask(Guid id)
        {
            return TaskService.StopTask(id);
        }

        public bool ConfigureMachine(Guid id)
        {
            return MachineConfigurationService.ConfigureMachine(id);
        }

        public int GetStatusOfConfiguring(Guid id)
        {
            return MachineConfigurationService.GetStatusOfConfiguring(id);
        }
    }
}