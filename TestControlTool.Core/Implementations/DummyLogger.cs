using System;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Doing nothing. 
    /// </summary>
    public class DummyLogger : ILogger
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Writes success message to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Success(string message)
        {
            
        }

        /// <summary>
        /// Writes info message to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Info(string message)
        {
            
        }

        /// <summary>
        /// Writes error message to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Error(string message)
        {
            
        }

        /// <summary>
        /// Writes message about exception to the log
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        public void Exception(string message, Exception exception)
        {
            
        }

        /// <summary>
        /// Writes only message (with out additional info) to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Message(string message)
        {
        }
    }
}