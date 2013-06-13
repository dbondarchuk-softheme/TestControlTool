using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Status of the task
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Never run
        /// </summary>
        Undefined,

        /// <summary>
        /// Running at the moment
        /// </summary>
        Running,

        /// <summary>
        /// Finished (successfully or run-time failed)
        /// </summary>
        Finished,

        /// <summary>
        /// Stopped by request (user or timeout)
        /// </summary>
        Stopped,

        /// <summary>
        /// Failed before start
        /// </summary>
        Failed,

        /// <summary>
        /// Task stopped due to timeout
        /// </summary>
        Timeout
    }

    /// <summary>
    /// Describes scheduled task for the machine
    /// </summary>
    public interface IScheduleTask
    {
        /// <summary>
        /// Task's id
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Task's name
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Task is enabled only after this date.
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// How often does the task starts. In Cron format
        /// </summary>
        string Frequency { get; set; }

        /// <summary>
        /// Task is enabled only before this date 
        /// </summary>
        DateTime EndTime { get; set; }

        /// <summary>
        /// Dtermines, will the task start due to the schedule
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Owner's Id
        /// </summary>
        Guid Owner { get; set; }

        /// <summary>
        /// Task's status
        /// </summary>
        TaskStatus Status { get; set; }

        /// <summary>
        /// Last run of the task
        /// </summary>
        DateTime LastRun { get; set; }
        
        /// <summary>
        /// Checks, if it's time to start task
        /// </summary>
        /// <param name="time">Time to check</param>
        /// <exception cref="ArgumentException">Wrong time format</exception>
        /// <returns>True - if it's time</returns>
        bool IsTimeToStart(DateTime time);

        /// <summary>
        /// Starts the task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<bool> Start(CancellationToken cancellationToken);

        /// <summary>
        /// Stops task
        /// </summary>
        void Stop();
    }
}
