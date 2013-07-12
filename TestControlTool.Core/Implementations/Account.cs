using System;
using System.Collections.Generic;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes system accounts
    /// </summary>
    public class Account : IAccount
    {
        /// <summary>
        /// Creates new account
        /// </summary>
        public Account()
        {
            Machines = new HashSet<IMachine>();
        }
    
        /// <summary>
        /// Account's id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Account's login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Account's password
        /// </summary>
        public string PasswordHash { get; set; }
    
        /// <summary>
        /// Account's machines
        /// </summary>
        public ICollection<IMachine> Machines { get; set; }

        /// <summary>
        /// Account's tasks
        /// </summary>
        public ICollection<IScheduleTask> Tasks { get; set; }

        /// <summary>
        /// Account's binded VM servers
        /// </summary>
        public ICollection<VMServer> VMServers { get; set; }
    }
}
