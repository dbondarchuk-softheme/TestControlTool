using System;
using System.Collections.Generic;

namespace TestControlTool.Core.Models
{
    public class TaskChildContainer
    {
        public IEnumerable<Guid> Machines { get; set; }

        public IEnumerable<ChildTaskModel> ChildTasks { get; set; }
    }
}