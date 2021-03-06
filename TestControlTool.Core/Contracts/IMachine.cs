﻿using System;

namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Appassure products types for the machine
    /// </summary>
    public enum MachineType
    {
        /// <summary>
        /// Appassure Core
        /// </summary>
        Core,

        /// <summary>
        /// Appassure Agent-X64
        /// </summary>
        Agent64,

        /// <summary>
        /// Appassure Agent-X86
        /// </summary>
        Agent32
    }

    /// <summary>
    /// Describes virtual machine
    /// </summary>
    public interface IMachine
    {
        /// <summary>
        /// Machine's Id
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Owner's Id
        /// </summary>
        Guid Owner { get; set; }

        /// <summary>
        /// Server's id
        /// </summary>
        Guid Server { get; set; }

        /// <summary>
        /// Machine's name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Appassure products types for the machine
        /// </summary>
        MachineType Type { get; set; }

        /// <summary>
        /// IP address of the machine
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// Host address of the machine
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Name for the user on this machine
        /// </summary>
        string UserName { get; set; }
        
        /// <summary>
        /// Password for the user on this machine
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Share folder on the machine
        /// </summary>
        string Share { get; set; }
    }
}
