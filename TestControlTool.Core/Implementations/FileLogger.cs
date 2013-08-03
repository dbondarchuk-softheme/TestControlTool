using System;
using System.IO;
using System.Text;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes logger, which writes logs to the file
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly StreamWriter _writer;

        private const string FormatString = "{0}. [{1}]. {2}";

        /// <summary>
        /// Creates new logger to the file
        /// </summary>
        /// <param name="file">Log file. If exists - owerwrites</param>
        /// <param name="append">If true - appends to the end of the log</param>
        public FileLogger(string file, bool append = false)
        {
            _writer = new StreamWriter(File.Open(file, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    AutoFlush = true
                };
        }

        /// <summary>
        /// Writes success message to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Success(string message)
        {
            _writer.WriteLine(string.Format(FormatString, DateTime.Now, "SUCCESS", message));
        }

        /// <summary>
        /// Writes info message to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Info(string message)
        {
            _writer.WriteLine(string.Format(FormatString, DateTime.Now, "INFO", message));
        }

        /// <summary>
        /// Writes error message to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Error(string message)
        {
            _writer.WriteLine(string.Format(FormatString, DateTime.Now, "ERROR", message));
        }

        /// <summary>
        /// Writes message about exception to the log
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        public void Exception(string message, Exception exception)
        {
            _writer.WriteLine(string.Format(FormatString, DateTime.Now, "EXCEPTION", message + "\n" + exception.Message + "\nStacktrace:\n" + exception.StackTrace));
        }

        /// <summary>
        /// Writes only message (with out additional info) to the log
        /// </summary>
        /// <param name="message">Message</param>
        public void Message(string message)
        {
            _writer.WriteLine(message);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}