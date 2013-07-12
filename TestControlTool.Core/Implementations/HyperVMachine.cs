using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes machine on the HyperV 
    /// </summary>
    public class HyperVMachine : IMachine
    {
        /// <summary>
        /// Machine's Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Owner's Id
        /// </summary>
        public Guid Owner { get; set; }

        /// <summary>
        /// Server's id
        /// </summary>
        public Guid Server { get; set; }

        /// <summary>
        /// Machine's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Appassure products types for the machine
        /// </summary>
        public MachineType Type { get; set; }

        /// <summary>
        /// IP address of the machine
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Host address of the machine
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Name for the user on this machine
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password for the user on this machine
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Share folder on the machine
        /// </summary>
        public string Share { get; set; }

        /// <summary>
        /// Name of the machine on the server
        /// </summary>
        public string VirtualMachineName { get; set; }

        /// <summary>
        /// Snapshot from which take the machine state
        /// </summary>
        public string Snapshot { get; set; }
    }
}