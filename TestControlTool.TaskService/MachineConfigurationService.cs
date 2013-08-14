using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;

namespace TestControlTool.TaskService
{
    public static class MachineConfigurationService
    {
        private static readonly EmailReportService EmailReportService = new EmailReportService();

        public static bool ConfigureMachine(MachineConfigurationModel machineConfigurationModel)
        {
            try
            {
                var task = new MachineConfigurationTask
                    {
                        MachineConfigurationModel = machineConfigurationModel
                    };

                Task.Factory.StartNew(task.Run).ContinueWith(
                    _ => EmailReportService.SendEmail(machineConfigurationModel.OwnerUserName, "TestControlTool. Machine configuring",
                                                      "Log of the '" + machineConfigurationModel.ComputerName +
                                                      "' configuring", new[] {task.LogName}));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
