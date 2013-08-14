using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using TestControlTool.Core.Models;

namespace TestControlTool.TaskService
{
    [ServiceContract]
    interface ITaskWcfService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/StartTask/?id={id}", ResponseFormat = WebMessageFormat.Json)]
        bool StartTask(Guid id);

        [OperationContract]
        [WebGet(UriTemplate = "/StopTask/?id={id}", ResponseFormat = WebMessageFormat.Json)]
        bool StopTask(Guid id);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/ConfigureMachine/", ResponseFormat = WebMessageFormat.Json)]
        bool ConfigureMachine(MachineConfigurationModel machineConfigurationModel);
    }
}
