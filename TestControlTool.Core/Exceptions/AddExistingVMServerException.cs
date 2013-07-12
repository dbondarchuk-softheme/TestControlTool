using System;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;

namespace TestControlTool.Core.Exceptions
{
    /// <summary>
    /// Generated, when such server is already presented in the database
    /// </summary>
    public class AddExistingVMServerException : Exception
    {
        /// <summary>
        /// Create's new AddExistingVMServerException with mathced message
        /// </summary>
        /// <param name="message">Exception message</param>
        public AddExistingVMServerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new AddExistingVMServerException with mathced message and inner exception
        /// </summary>
        /// <param name="message">Exception messag</param>
        /// <param name="innerException">Inner exception</param>
        public AddExistingVMServerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Create's new AddExistingVMServerException
        /// </summary>
        public AddExistingVMServerException()
            : base("Such server is already presented in the database")
        {
        }

        /// <summary>
        /// Create's new AddExistingVMServerException for trying to add matched server
        /// </summary>
        /// <param name="server">Server, which was tried to add</param>
        public AddExistingVMServerException(VMServer server)
            : base("Server with id = " + server.Id + " is already presented in the database")
        {
        }
    }
}
