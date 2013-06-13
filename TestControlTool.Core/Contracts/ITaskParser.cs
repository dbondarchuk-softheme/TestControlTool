using System;
using System.Collections.Generic;

namespace TestControlTool.Core.Contracts
{
    public interface ITaskParser
    {
        /// <summary>
        /// Parses tasks's child jobs from the xml files
        /// </summary>
        /// <param name="id">Id of the task</param>
        /// <param name="logger">Logger for the task</param>
        /// <returns>List of child tasks</returns>
        IEnumerable<IChildTask> Parse(Guid id, ILogger logger);
    }
}