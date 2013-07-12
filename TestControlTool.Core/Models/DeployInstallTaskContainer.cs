using System;
using System.Collections.Generic;
using TestControlTool.Core.Implementations;

namespace TestControlTool.Core.Models
{
    /// <summary>
    /// Type of the autodeploy job
    /// </summary>
    public enum DeployInstallType
    {
        /// <summary>
        /// Deploy machines and install products
        /// </summary>
        DeployInstall,

        /// <summary>
        /// Only deploy machines
        /// </summary>
        Deploy,

        /// <summary>
        /// Only install products
        /// </summary>
        Install
    }

    /// <summary>
    /// Contains info for the DeployInstall task
    /// </summary>
    public class DeployInstallTaskContainer
    {
        /// <summary>
        /// List of machine's id
        /// </summary>
        public List<Guid> Machines { get; set; }

        /// <summary>
        /// List of pair Type(VCenter or HyperV) -- File name
        /// </summary>
        public List<Pair<VMServerType, string>> Files { get; set; }

        /// <summary>
        /// Version to deploy
        /// </summary>
        public string BuildVersion { get; set; }

        /// <summary>
        /// Version to deploy
        /// </summary>
        public DeployInstallType DeployInstallType { get; set; }
    }

    /// <summary>
    /// Represents key-value pair
    /// </summary>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    public class Pair<K, V>
    {
        /// <summary>
        /// Key object
        /// </summary>
        public K Key { get; set; }

        /// <summary>
        /// Value object
        /// </summary>
        public V Value { get; set; }

        /// <summary>
        /// Creates new instance of key-value object
        /// </summary>
        public Pair(){}
 
        /// <summary>
        /// Creates new instance of key-value object
        /// </summary>
        /// <param name="key">Key object</param>
        /// <param name="value">Value object</param>
        public Pair(K key, V value)
        {
            Key = key;
            Value = value;
        } 
    }
}
