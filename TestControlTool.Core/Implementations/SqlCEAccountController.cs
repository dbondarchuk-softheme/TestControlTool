using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Exceptions;
using TestControlTool.Core.Implementations.SqlCEModels;

namespace TestControlTool.Core.Implementations
{
    public class SqlCEAccountController : IAccountController
    {
        private readonly AccountsEntities _database = new AccountsEntities();

        /// <summary>
        /// Get's all accounts
        /// </summary>
        public ICollection<IAccount> Accounts { get { return _database.Accounts.AsNoTracking().Include("Tasks").Include("Machines").ToList().Select(ToIAccount).ToList(); } }

        /// <summary>
        /// Get's all machines
        /// </summary>
        public ICollection<IMachine> Machines { get { return _database.Machines.AsNoTracking().Select(ToIMachine).ToList(); } }

        /// <summary>
        /// Get's all tasks
        /// </summary>
        public ICollection<IScheduleTask> Tasks { get { return _database.Tasks.AsNoTracking().ToList().Select(ToITask).ToList(); } }

        /// <summary>
        /// Get's all accounts (cached)
        /// </summary>
        public ICollection<IAccount> CachedAccounts { get; private set; }

        /// <summary>
        /// Get's all machines (cached)
        /// </summary>
        public ICollection<IMachine> CachedMachines { get; private set; }

        /// <summary>
        /// Get's all tasks (cached)
        /// </summary>
        public ICollection<IScheduleTask> CachedTasks { get; private set; }

        /// <summary>
        /// Creates new instance of the SqlCEAccountController
        /// </summary>
        public SqlCEAccountController()
        {
            CachedAccounts = new List<IAccount>(Accounts);
            CachedMachines = new List<IMachine>(Machines);
            CachedTasks = new List<IScheduleTask>(Tasks);
        }

        /// <summary>
        /// Adds account to the database
        /// </summary>
        /// <param name="account">Account to add</param>
        /// <exception cref="AddExistingAccountException">If account with such id or login is already presented in the database</exception>
        public void AddAccount(IAccount account)
        {
            try
            {
                _database.Accounts.Add(ToAccount(account));
                _database.SaveChanges();

                RefreshAccounts();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                throw new AddExistingAccountException(account);
            }
        }

        /// <summary>
        /// Add's machine to the database
        /// </summary>
        /// <param name="machine">Machine to add</param>
        /// <exception cref="NoSuchAccountException">If <paramref name="machine"/>'s owner wasn't found</exception>
        /// <exception cref="AddExistingMachineException">If <paramref name="machine"/> with such id is already presented in the database</exception>
        public void AddMachine(IMachine machine)
        {
            if (!CachedAccounts.Any(x => x.Id == machine.Owner)) throw new NoSuchAccountException(machine.Owner);

            try
            {
                _database.Machines.Add(ToMachine(machine));
                _database.SaveChanges();

                Refresh();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                throw new AddExistingMachineException(machine);
            }
        }

        /// <summary>
        /// Add's task to the database
        /// </summary>
        /// <param name="task">Task to add</param>
        /// <exception cref="NoSuchAccountException">If <paramref name="task"/>'s owner wasn't found</exception>
        /// <exception cref="AddExistingTaskException">If <paramref name="task"/> with such id is already presented in the database</exception>
        public void AddTask(IScheduleTask task)
        {
            if (!CachedAccounts.Any(x => x.Id == task.Owner)) throw new NoSuchAccountException(task.Owner);

            if (task.StartTime > task.EndTime) throw new ArgumentException("Start time sholud be less than end time");

            try
            {
                _database.Tasks.Add(ToTask(task));
                _database.SaveChanges();

                Refresh();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException e)
            {
                throw new AddExistingTaskException(task);
            }

            Refresh();
        }

        /// <summary>
        /// Changes password for the account
        /// </summary>
        /// <param name="accountId">Account id</param>
        /// <param name="newPassword">New password to set</param>
        /// <exception cref="NoSuchAccountException">If account with <paramref name="accountId"/> wasn't found</exception>
        public void ChangePassword(Guid accountId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException("newPassword");
            }

            var accountString = accountId.ToString();
            var account = _database.Accounts.FirstOrDefault(x => x.Id == accountString);

            if (account == null) throw new NoSuchAccountException(accountId);

            account.PasswordHash = newPassword;

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Edit's machine with such id
        /// </summary>
        /// <param name="id">Id of the machine to edit</param>
        /// <param name="machine">New machine info (new id and owner doesn't take effect)</param>
        /// <exception cref="NoSuchMachineException">If machine with <paramref name="id"/> wasn't found</exception>
        public void EditMachine(Guid id, IMachine machine)
        {
            var idString = id.ToString();
            var machineInDatabase = _database.Machines.FirstOrDefault(x => x.Id == idString);

            if (machineInDatabase == null) throw new NoSuchMachineException(id);

            machineInDatabase.Name = machine.Name;
            machineInDatabase.Host = machine.Host;
            machineInDatabase.UserName = machine.UserName;
            machineInDatabase.Password = machine.Password;
            machineInDatabase.Share = machine.Share;
            machineInDatabase.Address = machine.Address;
            machineInDatabase.TemplateInventoryPath = machine.TemplateInventoryPath;
            machineInDatabase.TemplateVMName = machine.TemplateVMName;
            machineInDatabase.VirtualMachineVMName = machine.VirtualMachineVMName;
            machineInDatabase.VirtualMachineResourcePool = machine.VirtualMachineResourcePool;
            machineInDatabase.VirtualMachineInventoryPath = machine.VirtualMachineInventoryPath;
            machineInDatabase.VirtualMachineDatastore = machine.VirtualMachineDatastore;
            machineInDatabase.Type = machine.Type.ToString();

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Remove's machine with such id
        /// </summary>
        /// <param name="id">Id of the machine to remove</param>
        /// <exception cref="NoSuchMachineException">If machine with <paramref name="id"/> wasn't found</exception>
        public void RemoveMachine(Guid id)
        {
            var idString = id.ToString();
            var machineInDatabase = _database.Machines.FirstOrDefault(x => x.Id == idString);

            if (machineInDatabase == null) throw new NoSuchMachineException(id);

            _database.Machines.Remove(machineInDatabase);

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Edit's task with such id
        /// </summary>
        /// <param name="id">Id of the task to edit</param>
        /// <param name="task">New task info (new id and machine doesn't take effect)</param>
        /// <exception cref="NoSuchTaskException">If task with <paramref name="id"/> wasn't found</exception>
        public void EditTask(Guid id, IScheduleTask task)
        {
            if (task.StartTime > task.EndTime) throw new ArgumentException("Start time sholud be less than end time");

            var idString = id.ToString();
            var taskInDatabase = _database.Tasks.FirstOrDefault(x => x.Id == idString);

            if (taskInDatabase == null) throw new NoSuchTaskException(id);

            taskInDatabase.StartTime = task.StartTime;
            taskInDatabase.EndTime = task.EndTime;
            taskInDatabase.Frequency = task.Frequency;
            taskInDatabase.IsEnabled = task.IsEnabled;
            taskInDatabase.Name = task.Name;

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Remove's task with such id
        /// </summary>
        /// <param name="id">Id of the task to remove</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        public void RemoveTask(Guid id)
        {
            var idString = id.ToString();
            var taskInDatabase = _database.Tasks.FirstOrDefault(x => x.Id == idString);

            if (taskInDatabase == null) throw new NoSuchTaskException(id);

            _database.Tasks.Remove(taskInDatabase);

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Sets status of the task
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="status">Task's status</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        public void SetTaskStatus(Guid id, TaskStatus status)
        {
            var idString = id.ToString();
            var taskInDatabase = _database.Tasks.FirstOrDefault(x => x.Id == idString);

            if (taskInDatabase == null) throw new NoSuchTaskException(id);

            taskInDatabase.Status = status.ToString();

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Sets task's last run time
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="time">Last run time</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        public void SetTaskLastRun(Guid id, DateTime time)
        {
            var idString = id.ToString();
            var taskInDatabase = _database.Tasks.FirstOrDefault(x => x.Id == idString);

            if (taskInDatabase == null) throw new NoSuchTaskException(id);

            taskInDatabase.LastRun = time;

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Sets task's last run time and status
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="status">Task's status</param>
        /// <param name="time">Last run time</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        public void SetTaskStatusAndLastRun(Guid id, TaskStatus status, DateTime time)
        {
            var idString = id.ToString();
            var taskInDatabase = _database.Tasks.FirstOrDefault(x => x.Id == idString);

            if (taskInDatabase == null) throw new NoSuchTaskException(id);

            taskInDatabase.Status = status.ToString();
            taskInDatabase.LastRun = time;

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Sets task's enabled status
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="enabled">Is task enabled or not</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        public void SetTaskEnabled(Guid id, bool enabled)
        {
            var idString = id.ToString();
            var taskInDatabase = _database.Tasks.FirstOrDefault(x => x.Id == idString);

            if (taskInDatabase == null) throw new NoSuchTaskException(id);

            taskInDatabase.IsEnabled = enabled;

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Check's if account login and password are valid
        /// </summary>
        /// <param name="account">Account to check</param>
        /// <returns>True - if valid</returns>
        public bool IsValidAccount(IAccount account)
        {
            return Accounts.Any(x => x.Login == account.Login && x.PasswordHash == account.PasswordHash);
        }
        
        /// <summary>
        /// Refreshes cached collections.
        /// </summary>
        public void Refresh()
        {
            RefreshAccounts();
        }

        private void RefreshAccounts()
        {
            lock (CachedAccounts)
            {
                CachedAccounts = new List<IAccount>(Accounts);
            }

            RefreshMachines();
            RefreshTasks();
        }

        private void RefreshMachines()
        {
            lock (CachedMachines)
            {
                CachedMachines = new List<IMachine>(Machines);                
            }
        }

        private void RefreshTasks()
        {
            lock (CachedTasks)
            {
                CachedTasks = new List<IScheduleTask>(Tasks);
            }
        }

        private static AccountModel ToAccount(IAccount account)
        {
            return new AccountModel
                {
                    Id = account.Id.ToString(),
                    Login = account.Login,
                    PasswordHash = account.PasswordHash,
                    Machines = account.Machines != null ? (ICollection<MachineModel>) account.Machines.Select(ToMachine).ToList() : new Collection<MachineModel>(),
                    Tasks = account.Tasks != null ? (ICollection<TaskModel>) account.Tasks.Select(ToTask).ToList() : new Collection<TaskModel>()
                };
        }

        private static MachineModel ToMachine(IMachine machine)
        {
            return new MachineModel
                {
                    Owner = machine.Owner.ToString(),
                    Name = machine.Name,
                    Host = machine.Host,
                    Address = machine.Address,
                    UserName = machine.UserName,
                    Password = machine.Password,
                    Share = machine.Share,
                    Id = machine.Id.ToString(),
                    TemplateInventoryPath = machine.TemplateInventoryPath,
                    TemplateVMName = machine.TemplateVMName,
                    VirtualMachineVMName = machine.VirtualMachineVMName,
                    VirtualMachineResourcePool = machine.VirtualMachineResourcePool,
                    VirtualMachineInventoryPath = machine.VirtualMachineInventoryPath,
                    VirtualMachineDatastore = machine.VirtualMachineDatastore,
                    Type = machine.Type.ToString()
                };
        }

        private static TaskModel ToTask(IScheduleTask task)
        {
            return new TaskModel
            {
                Id = task.Id.ToString(),
                StartTime = task.StartTime,
                Frequency = task.Frequency,
                EndTime = task.EndTime,
                IsEnabled = task.IsEnabled,
                Owner = task.Owner.ToString(),
                Name = task.Name,
                Status = task.Status.ToString(),
                LastRun = task.LastRun
            };
        }

        private static IAccount ToIAccount(AccountModel account)
        {
            return new Account
            {
                Id = new Guid(account.Id),
                Login = account.Login,
                PasswordHash = account.PasswordHash,
                Machines = account.Machines.Select(ToIMachine).ToList(),
                Tasks = account.Tasks.Select(ToITask).ToList()
            };
        }

        private static IMachine ToIMachine(MachineModel machine)
        {
            return new Machine
            {
                Id = new Guid(machine.Id),
                Owner = new Guid(machine.Owner),
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                TemplateInventoryPath = machine.TemplateInventoryPath,
                TemplateVMName = machine.TemplateVMName,
                VirtualMachineVMName = machine.VirtualMachineVMName,
                VirtualMachineResourcePool = machine.VirtualMachineResourcePool,
                VirtualMachineInventoryPath = machine.VirtualMachineInventoryPath,
                VirtualMachineDatastore = machine.VirtualMachineDatastore,
                Type = (MachineType)Enum.Parse(typeof(MachineType), machine.Type, true)
            };
        }

        private static IScheduleTask ToITask(TaskModel task)
        {
            return new ScheduleTask
                {
                    Id = new Guid(task.Id),
                    StartTime = task.StartTime,
                    Frequency = task.Frequency,
                    EndTime = task.EndTime,
                    IsEnabled = task.IsEnabled,
                    Owner = new Guid(task.Owner),
                    Name = task.Name,
                    LastRun = task.LastRun,
                    Status = (TaskStatus) Enum.Parse(typeof (TaskStatus), task.Status, true),
                };
        }
    }
}
