using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using BootstrapSupport;
using Newtonsoft.Json.Linq;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;

namespace TestControlTool.Web.Models
{
    public class TaskModel
    {
        public static readonly string[] ShownProperties = new[] { "Name", "Status", "LastRun"};

        public static readonly string[] NotShownProperties = new[] { "Id", "Owner", "Frequency", "IsEnabled", "StartTime", "EndTime" };

        [HiddenInput(DisplayValue = false)]
        public Guid Id { get; set; }

        [Link(Action = "Edit", Controller = "Task", Title = "Edit Task")]
        [Required]
        [Remote("ValidateUniqueTaskName", "Validation", ErrorMessage = "Such name is already used", AdditionalFields = "Id")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Link(Action = "Edit", Controller = "Task", Title = "Edit Task")]
        [Display(Name = "Schedule")]
        public string Schedule { get { return GetSchedule(); }}

        [Link(Action = "Edit", Controller = "Task", Title = "Edit Task")]
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start time")]
        public DateTime StartTime { get; set; }

        [Link(Action = "Edit", Controller = "Task", Title = "Edit Task")]
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start time")]
        public DateTime EndTime { get; set; }

        [Required]
        [Remote("ValidateFrequency", "Validation", ErrorMessage = "Wrong frequency format")]
        public string Frequency { get; set; }

        [Link(Action = "Pause", Controller = "Task", Title = "Pause Task")]
        [Display(Name = "Schedule Start")]
        public bool IsEnabled { get; set; }

        [Link(Action = "ViewLogs", Controller = "Task", Title = "View Logs")]
        [Display(Name = "Status")]
        public TaskStatus Status { get; set; }

        [Link(Action = "ViewLogs", Controller = "Task", Title = "View Logs")]
        [Display(Name = "Last Run")]
        public DateTime LastRun { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid Owner { get; set; }

        public TaskModel()
        {
            Id = Guid.NewGuid();
            StartTime = DateTime.Now;
            EndTime = DateTime.Now.AddYears(1);
            LastRun = new DateTime(1970, 1, 1);
            IsEnabled = true;
            Frequency = "* * * *";
        }

        public TaskModel(string jsonModel)
        {
            var jObject = JObject.Parse(jsonModel);

            Name = (jObject["name"] == null) ? "" : jObject["name"].ToString();
            Id = (jObject["id"] == null) ? Guid.NewGuid() : new Guid(jObject["id"].ToString());
            StartTime = (jObject["startTime"] == null) ? DateTime.Now : DateTime.Parse(jObject["startTime"].ToString());
            EndTime = (jObject["endTime"] == null) ? DateTime.Now.AddYears(1) : DateTime.Parse(jObject["endTime"].ToString());
            Frequency = (jObject["frequency"] == null) ? "* * * *" : jObject["frequency"].ToString();
            IsEnabled = (jObject["isEnabled"] == null) ? false : bool.Parse(jObject["isEnabled"].ToString());
            Status = (jObject["status"] == null) ? TaskStatus.Undefined : (TaskStatus)Enum.Parse(typeof(TaskStatus), jObject["status"].ToString(), true);
            LastRun = (jObject["lastRun"] == null) ? new DateTime(1970, 1, 1) : DateTime.Parse(jObject["lastRun"].ToString()); ;
            Owner = (jObject["owner"] == null) ? Guid.Empty : new Guid(jObject["owner"].ToString());
        }

        public static TaskModel FromITask(IScheduleTask task)
        {
            return new TaskModel
                {
                    Id = task.Id,
                    Owner = task.Owner,
                    Name = task.Name,
                    StartTime = task.StartTime,
                    EndTime = task.EndTime,
                    Frequency = task.Frequency,
                    IsEnabled = task.IsEnabled,
                    Status = task.Status,
                    LastRun = task.LastRun
                };
        }

        public IScheduleTask ToITask()
        {
            return new ScheduleTask
                {
                    Id = Id,
                    Name = Name,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Frequency = Frequency,
                    IsEnabled = IsEnabled,
                    Owner = Owner,
                    Status = Status,
                    LastRun = LastRun
                };
        }

        private string GetSchedule()
        {
            var days = Enum.GetNames(typeof (DayOfWeek));
            var frequency = Frequency.Split(' ')[2];

            if (frequency == "*") return "Everyday at " + StartTime.ToString("hh:mm tt"); ;
            if (frequency == "!") return "Never";

            var intervals = ScheduleTask.GetInterval(frequency, 2).ToArray();

           var repetition = intervals.Aggregate("", (current, t) => current + (days[t == 7 ? 0 : t] + ", "));

            repetition = repetition.Trim(',', ' ');

            repetition += " at " + StartTime.ToString("hh:mm tt"); ;

            return repetition;
        }
    }
}