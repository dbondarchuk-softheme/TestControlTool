using System;

namespace TestControlTool.Core.Exceptions
{
    public class NoSuchTaskException : Exception
    {
        /// <summary>
        /// Create's new NoSuchTaskException with matched message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NoSuchTaskException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new NoSuchTaskException for such task ID
        /// </summary>
        /// <param name="taskId">Machine id to search</param>
        public NoSuchTaskException(Guid taskId)
            : base("No task with id = " + taskId + " was found in the database")
        {
        }
    }
}
