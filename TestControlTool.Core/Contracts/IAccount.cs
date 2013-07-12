using System;
using System.Collections.Generic;
using TestControlTool.Core.Implementations;

namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Describes user account
    /// </summary>
    public interface IAccount
    {
        /// <summary>
        /// User's id
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// User's login
        /// </summary>
        string Login { get; }

        /// <summary>
        /// Hash of the user's password
        /// </summary>
        string PasswordHash { get; }

        /// <summary>
        /// List of users machines
        /// </summary>
        ICollection<IMachine> Machines { get; }

        /// <summary>
        /// Account's tasks
        /// </summary>
        ICollection<IScheduleTask> Tasks { get; }

        /// <summary>
        /// Account's binded VM servers
        /// </summary>
        ICollection<VMServer> VMServers { get; }
    }
}
