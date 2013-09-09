using System.Threading.Tasks;
using TestControlTool.Core.Implementations;
using TestControlTool.Core.Models;

namespace TestControlTool.TaskService
{
    public static class MachineConfigurationService
    {
        public static bool ConfigureMachine(MachineConfigurationModel machineConfigurationModel)
        {
            try
            {
                var task = new MachineConfigurationTask
                    {
                        MachineConfigurationModel = machineConfigurationModel
                    };

                Task.Factory.StartNew(task.Run).ContinueWith(
                    _ => EmailReportService.SendEmail(new[] {machineConfigurationModel.OwnerUserName}, "TestControlTool. Machine configuring",
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
