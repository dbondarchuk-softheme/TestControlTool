using System;
using System.Xml.Serialization;

namespace TestControlTool.Core.Models
{
    /// <summary>
    /// Incapsulates child task
    /// </summary>
    [Serializable]
    public class ChildTaskModel
    {
        /// <summary>
        /// Task type
        /// </summary>
        [XmlAttribute]
        public TaskType TaskType { get; set; }

        /// <summary>
        /// Xml file with task
        /// </summary>
        [XmlElement]
        public string File { get; set; }

        /// <summary>
        /// Task name
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }
    }

    /// <summary>
    /// Describes type of the task
    /// </summary>
    [Serializable]
    public enum TaskType
    {
        /// <summary>
        /// Deploy, Install or DeployInstall task
        /// </summary>
        DeployInstall,

        /// <summary>
        /// Test suite of WGA for trunk task 
        /// </summary>
        UISuiteTrunk,
        
        /// <summary>
        /// Test suite of WGA for release task 
        /// </summary>
        UISuiteRelease,

        /// <summary>
        /// Suite for the backend automation (trunk)
        /// </summary>
        BackendSuiteTrunk,

        /// <summary>
        /// Suite for the backend automation (release)
        /// </summary>
        BackendSuiteRelease,
    }
}
