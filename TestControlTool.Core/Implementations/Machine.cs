using System;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes virtual machine
    /// </summary>
    public class Machine : IMachine
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
        /// Name of the template of the machine
        /// </summary>
        public string TemplateVMName { get; set; }

        /// <summary>
        /// Inventory path of the template
        /// </summary>
        public string TemplateInventoryPath { get; set; }

        /// <summary>
        /// Name of the virtual machine on the VCenter
        /// </summary>
        public string VirtualMachineVMName { get; set; }

        /// <summary>
        /// Inventory path of the virtual machine
        /// </summary>
        public string VirtualMachineInventoryPath { get; set; }

        /// <summary>
        /// Resource pool of the virtual machine
        /// </summary>
        public string VirtualMachineResourcePool { get; set; }

        /// <summary>
        /// Datastore for the virtual machine
        /// </summary>
        public string VirtualMachineDatastore { get; set; }

        /// <summary>
        /// Owner's account
        /// </summary>
        public Account Account { get; set; }
    }
}
