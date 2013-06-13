using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestControlTool.TaskService
{
    public interface IReportService
    {
        void ProcessReport(Guid taskId, string taskName, string ownerName);
    }
}
