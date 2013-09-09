using System;
using System.IO;
using System.ServiceModel;
using TestControlTool.Core.Models;

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

        public bool ConfigureMachine(MachineConfigurationModel machineConfigurationModel)
        {
            return MachineConfigurationService.ConfigureMachine(machineConfigurationModel);
        }

        public void SendEmail(string user, string subject, string message)
        {
            EmailService.SendEmail(user, subject, message);
        }

        public void SendEmailToAll(string subject, string message)
        {
            EmailService.SendEmailToAll(subject, message);
        }
    }
}