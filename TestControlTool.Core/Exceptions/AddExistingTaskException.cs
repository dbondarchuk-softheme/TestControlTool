using System;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Exceptions
{
    /// <summary>
    /// Generated, when such account is already presented in the database
    /// </summary>
    public class AddExistingTaskException : Exception
    {
        /// <summary>
        /// Create's new AddExistingTaskException with mathced message
        /// </summary>
        /// <param name="message">Exception message</param>
        public AddExistingTaskException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new AddExistingTaskException with mathced message and inner exception
        /// </summary>
        /// <param name="message">Exception messag</param>
        /// <param name="innerException">Inner exception</param>
        public AddExistingTaskException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Create's new AddExistingTaskException
        /// </summary>
        public AddExistingTaskException()
            : base("Such task is already presented in the database")
        {
        }

        /// <summary>
        /// Create's new AddExistingMachineException for trying to add matched machine
        /// </summary>
        /// <param name="task">Account, which was tried to add</param>
        public AddExistingTaskException(IScheduleTask task)
            : base("Task with id = " + task.Id + " is already presented in the database")
        {
        }
    }
}
