using System;

namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Describes basic logger
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Writes success message to the log
        /// </summary>
        /// <param name="message">Message</param>
        void Success(string message);

        /// <summary>
        /// Writes info message to the log
        /// </summary>
        /// <param name="message">Message</param>
        void Info(string message);

        /// <summary>
        /// Writes error message to the log
        /// </summary>
        /// <param name="message">Message</param>
        void Error(string message);

        /// <summary>
        /// Writes message about exception to the log
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        void Exception(string message, Exception exception);
    }
}
