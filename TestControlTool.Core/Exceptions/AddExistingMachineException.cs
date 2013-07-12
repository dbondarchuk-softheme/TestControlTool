using System;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Exceptions
{
    /// <summary>
    /// Generated, when such machine is already presented in the database
    /// </summary>
    public class AddExistingMachineException : Exception
    {
        /// <summary>
        /// Create's new AddExistingMachineException with mathced message
        /// </summary>
        /// <param name="message">Exception message</param>
        public AddExistingMachineException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new AddExistingMachineException with mathced message and inner exception
        /// </summary>
        /// <param name="message">Exception messag</param>
        /// <param name="innerException">Inner exception</param>
        public AddExistingMachineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Create's new AddExistingMachineException
        /// </summary>
        public AddExistingMachineException()
            : base("Such machine is already presented in the database")
        {
        }

        /// <summary>
        /// Create's new AddExistingMachineException for trying to add matched machine
        /// </summary>
        /// <param name="machine">Account, which was tried to add</param>
        public AddExistingMachineException(IMachine machine)
            : base("Machine with id = " + machine.Id + " is already presented in the database")
        {
        }
    }
}
