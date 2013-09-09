using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TaskStatus = TestControlTool.Core.Contracts.TaskStatus;

namespace TestControlTool.TaskService
{
    public static class TaskService
    {
        private static readonly IAccountController AccountController = CastleResolver.Resolve<IAccountController>();
        private static readonly IReportService ReportService = new EmailReportService();

        private static readonly ConcurrentDictionary<Guid, Task> Tasks = new ConcurrentDictionary<Guid, Task>();
        private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> CancellationTokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>(); 

        public static bool RunTask(Guid id)
        {
            try
            {
                var task = AccountController.Tasks.SingleOrDefault(x => x.Id == id && x.Status != TaskStatus.Running);

                if (task == null) return false;

                lock (AccountController)
                {
                    AccountController.SetTaskStatusAndLastRun(task.Id, TaskStatus.Running, DateTime.Now);
                }

                var cancellationTokenSource = new CancellationTokenSource();

                CancellationTokenSources.AddOrUpdate(id, guid => cancellationTokenSource, (guid, source) => cancellationTokenSource);

                var runningTask = task.Start(cancellationTokenSource.Token);

                Tasks.AddOrUpdate(id, guid => runningTask, (guid, task1) => runningTask);

                runningTask.ContinueWith(t =>
                    {
                        Task taskToDelete;
                        CancellationTokenSource tokenToDelete;
                        Tasks.TryRemove(id, out taskToDelete);
                        CancellationTokenSources.TryRemove(id, out tokenToDelete);

                        if (!tokenToDelete.IsCancellationRequested)
                        {
                            lock (AccountController)
                            {
                                AccountController.SetTaskStatus(id, t.Result ? TaskStatus.Finished : TaskStatus.Failed);
                            }
                        }

                        var taskInfo = AccountController.CachedTasks.Single(x => x.Id == id);

                        ReportService.ProcessReport(id, taskInfo.Name, AccountController.CachedAccounts.Single(x => x.Id == taskInfo.Owner).Login);
                    });

                return true;
            }
            catch(Exception)
            {
                AccountController.SetTaskStatus(id, TaskStatus.Failed);
                return false;
            }
        }

        public static bool StopTask(Guid id, bool timeout = false)
        {
            try
            {
                var task = AccountController.Tasks.SingleOrDefault(x => x.Id == id && x.Status == TaskStatus.Running);

                if (task == null) return false;

                Task runningTask;
                CancellationTokenSource cancellationTokenSource;

                if (Tasks.TryGetValue(id, out runningTask) && CancellationTokenSources.TryGetValue(id, out cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                }

                task.Stop();
                
                lock (AccountController)
                {
                    AccountController.SetTaskStatus(task.Id, timeout ? TaskStatus.Timeout : TaskStatus.Stopped);
                }
                
                return true;
            }
            catch (Exception)
            {

                lock (AccountController)
                {
                    AccountController.SetTaskStatus(id, TaskStatus.Failed);
                }

                return false;
            }
        }
    }
}
