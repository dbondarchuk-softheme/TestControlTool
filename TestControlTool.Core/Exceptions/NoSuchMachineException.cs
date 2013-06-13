using System;

namespace TestControlTool.Core.Exceptions
{
    public class NoSuchMachineException : Exception
    {
        /// <summary>
        /// Create's new NoSuchMachineException with matched message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NoSuchMachineException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new NoSuchMachineException for such machine ID
        /// </summary>
        /// <param name="machineId">Machine id to search</param>
        public NoSuchMachineException(Guid machineId)
            : base("No machine with id = " + machineId + " was found in the database")
        {
        }
    }
}
