using System;

namespace TestControlTool.Core.Exceptions
{
    public class NoSuchVMServerException : Exception
    {
        /// <summary>
        /// Create's new NoSuchVMServerException with matched message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NoSuchVMServerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new NoSuchVMServerException for such server ID
        /// </summary>
        /// <param name="serverId">Machine id to search</param>
        public NoSuchVMServerException(Guid serverId)
            : base("No VM server with id = " + serverId + " was found in the database")
        {
        }
    }
}
