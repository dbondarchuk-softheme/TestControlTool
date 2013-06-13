using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TaskStatus = TestControlTool.Core.Contracts.TaskStatus;
using Timer = System.Timers.Timer;

namespace TestControlTool.TaskService
{
    partial class SchedulerService : ServiceBase
    {
        private readonly IAccountController _accountController = CastleResolver.Resolve<IAccountController>();

        private readonly Timer _refreshTimer = new Timer(1000000);
        private readonly Timer _taskTimer = new Timer(60000);
        private readonly static TimeSpan MaximalDuration = new TimeSpan(6, 0, 0);

        private ServiceHost _serviceHost;

        public SchedulerService()
        {
            InitializeComponent();
            
            _refreshTimer.Elapsed += RefreshTasks;
            _taskTimer.Elapsed += TaskTimerOnElapsed;

            _taskTimer.Start();
            _refreshTimer.Start();
        }

        private void TaskTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var task in _accountController.CachedTasks.Where(x => x.IsTimeToStart(DateTime.Now) && x.Status != TaskStatus.Running && x.IsEnabled))
            {
                TaskService.RunTask(task.Id);
            }

            foreach (var task in _accountController.CachedTasks.Where(x => (DateTime.Now - x.LastRun) > MaximalDuration && x.Status == TaskStatus.Running))
            {
                TaskService.StopTask(task.Id, true);
            }
        }

        public void StartService(string[] args)
        {
            OnStart(args);
        }

        public void StopService()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            _accountController.Refresh();

            if (_serviceHost != null)
            {
                _serviceHost.Close();
            }

            _serviceHost = new ServiceHost(new TaskWcfService());

            _serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
        }

        private void RefreshTasks(object sender, ElapsedEventArgs e)
        {
            _accountController.Refresh();
        }
    }
}
