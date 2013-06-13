using System.Collections.Generic;
using TestControlTool.Core.Models;

namespace TestControlTool.Web.Models
{
    public class TaskChildsModel
    {
        public IEnumerable<ChildTaskModel> ChildTasks { get; set; }

        public TaskModel Task { get; set; }
    }
}