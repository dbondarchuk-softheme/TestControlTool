using System;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Type of the virtual machine
    /// </summary>
    public enum VMServerType
    {
        /// <summary>
        /// HyperV machine
        /// </summary>
        HyperV,

        /// <summary>
        /// VCenter machine
        /// </summary>
        VCenter
    }

    /// <summary>
    /// Describes virtual machines server
    /// </summary>
    public struct VMServer
    {
        /// <summary>
        /// Id of the server
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Owner's id
        /// </summary>
        public Guid Owner { get; set; }

        /// <summary>
        /// Virtual machines server address
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Tyoe of the server
        /// </summary>
        public VMServerType Type { get; set; }

        /// <summary>
        /// Virtual machines server user
        /// </summary>
        public string ServerUsername { get; set; }

        /// <summary>
        /// Virtual machines server user's password
        /// </summary>
        public string ServerPassword { get; set; }
    }
}
