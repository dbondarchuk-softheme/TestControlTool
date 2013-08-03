using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

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
        [WebGet(UriTemplate = "/ConfigureMachine/?id={id}", ResponseFormat = WebMessageFormat.Json)]
        bool ConfigureMachine(Guid id);

        [OperationContract]
        [WebGet(UriTemplate = "/GetStatusOfConfiguring/?id={id}", ResponseFormat = WebMessageFormat.Json)]
        int GetStatusOfConfiguring(Guid id);
    }
}
