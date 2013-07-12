using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestControlTool.Core.Contracts;
using TaskStatus = TestControlTool.Core.Contracts.TaskStatus;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes scheduled task for the machine
    /// </summary>
    public class ScheduleTask : IScheduleTask
    {
        /// <summary>
        /// Task's id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Task's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Task is enabled only after this date.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// How often does the task starts. In Cron format
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// Task is enabled only before this date 
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Dtermines, will the task start due to the schedule
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Owner's Id
        /// </summary>
        public Guid Owner { get; set; }

        /// <summary>
        /// Task's status
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// Last run of the task
        /// </summary>
        public DateTime LastRun { get; set; }

        /// <summary>
        /// Owner's account
        /// </summary>
        public Account Account { get; set; }
        
        /// <summary>
        /// Checks, if it's time to start task
        /// </summary>
        /// <param name="time">Time to check</param>
        /// <exception cref="ArgumentException">Wrong time format</exception>
        /// <returns>True - if it's time</returns>
        public bool IsTimeToStart(DateTime time)
        {
            if (time < StartTime || time > EndTime || !IsEnabled || time.Minute != StartTime.Minute || time.Hour != StartTime.Hour) return false;

            var timeParts = Frequency.Split(' ');

            if (timeParts.Length == 0 || timeParts.Length > 4)
            {
                throw new ArgumentException("Wrong frequency format = " + Frequency);
            }

            var dayOfWeek = time.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)time.DayOfWeek;

            var intervals = new[] { time.Day, time.Month, dayOfWeek, time.Year };

            var result = true; //Such usage, cause need validator for the Frequncy string

            for (var i = 0; i < 4; i++)
            {
                if (!GetInterval(timeParts[i], i).Contains(intervals[i])) result = false;
            }

            return result;
        }

        /// <summary>
        /// Starts the task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<bool> Start(CancellationToken cancellationToken)
        {
            return Start(CastleResolver.Resolve<ITaskParser>(), cancellationToken);
        }

        /// <summary>
        /// Starts the task
        /// </summary>
        /// <param name="parser">Task parser to use</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task<bool> Start(ITaskParser parser, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var logger = new FileLogger(ConfigurationManager.AppSettings["LogsFolder"] + "\\" + Id + ".log");

                try
                {
                    var childTasks = parser.Parse(Id, logger);

                    foreach (var childTask in childTasks)
                    {
                        childTask.Run();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message + "\n" + e.StackTrace);
                    File.WriteAllText(@"D:\tasklog.txt", e.Message + "\n" + e.StackTrace);

                    return false;
                }

                return true;
            }, cancellationToken);
        }

        /// <summary>
        /// Stops the task
        /// </summary>
        public void Stop()
        {
            Stop(CastleResolver.Resolve<ITaskParser>());
        }

        /// <summary>
        /// Stops the task
        /// </summary>
        /// <param name="parser">Task parser to use</param>
        public void Stop(ITaskParser parser)
        {
            try
            {
                var logger = new FileLogger(ConfigurationManager.AppSettings["LogsFolder"] + "\\" + Id + ".log", true);
                var childTasks = parser.Parse(Id, logger);

                foreach (var childTask in childTasks)
                {
                    childTask.Stop();
                }
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Parses the StartTime periods
        /// </summary>
        /// <param name="value">Value to parse</param>
        /// <param name="period">Period (month, day etc)</param>
        /// <returns>Array of possible time values</returns>
        public static IEnumerable<int> GetInterval(string value, int period)
        {
            var maximal = 0;

            #region Maximal Period

            switch (period)
            {
                case 0:
                    maximal = 31;
                    break;

                case 1:
                    maximal = 12;
                    break;

                case 2:
                    maximal = 7;
                    break;

                case 3:
                    maximal = 3000;
                    break;
            }

            #endregion

            if (value == "*") return GenerateInterval(1, maximal);
            if (value == "!") return new[] { -1 };

            var intervals = value.Split(',');

            if (intervals.Length > 1)
            {
                return intervals.Aggregate(new List<int>(), (ints, s) =>
                    {
                        ints.AddRange(GetInterval(s, period));
                        return ints;
                    }).ToArray();
            }

            var duration = value.Split('-');

            if (duration.Length == 2)
            {
                int left;
                int right;

                if ((!int.TryParse(duration[0], out left) || !int.TryParse(duration[1], out right))
                    || (left < 0 || right > maximal || right < left))
                {
                    throw new ArgumentException("Wrong time format. " + value, "value");
                }

                return GenerateInterval(left, right);
            }

            int single;

            if ((!int.TryParse(value, out single) || (single < 0 || single > maximal)))
            {
                throw new ArgumentException("Wrong time format. " + value, "value");
            }

            return new[] { single };
        }

        /// <summary>
        /// Generates array of sequence from <paramref name="start"/> to <paramref name="end"/>
        /// </summary>
        /// <param name="start">Sequence's start</param>
        /// <param name="end">Sequence's end (included)</param>
        /// <returns>Array of sequence from <paramref name="start"/> to <paramref name="end"/></returns>
        public static IEnumerable<int> GenerateInterval(int start, int end)
        {
            var result = new List<int>();

            for (var i = start; i <= end; i++)
            {
                result.Add(i);
            }

            return result;
        }
    }
}
