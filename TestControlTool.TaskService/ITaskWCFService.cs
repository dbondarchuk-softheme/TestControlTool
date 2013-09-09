using System;
using System.ServiceModel;
using System.ServiceModel.Web;
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

        [OperationContract]
        [WebGet(UriTemplate = "/SendEmail/?user={user}&subject={subject}&message={message}", ResponseFormat = WebMessageFormat.Json)]
        void SendEmail(string user, string subject, string message);

        [OperationContract]
        [WebGet(UriTemplate = "/SendEmailToAll/?subject={subject}&message={message}", ResponseFormat = WebMessageFormat.Json)]
        void SendEmailToAll(string subject, string message);
    }
}
