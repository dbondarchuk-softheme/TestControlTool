using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public ICollection<IAccount> Accounts { get { return _database.Accounts.AsNoTracking().Include("Tasks").Include("HyperVMachines").Include("VCenterMachines").Include("Servers").ToList().Select(ToIAccount).ToList(); } }

        /// <summary>
        /// Get's all machines
        /// </summary>
        public ICollection<IMachine> Machines { get { return _database.VCenterMachines.AsNoTracking().Select(ToIMachine).Union(_database.HyperVMachines.AsNoTracking().Select(ToIMachine)).ToList(); } }

        /// <summary>
        /// Get's all tasks
        /// </summary>
        public ICollection<IScheduleTask> Tasks { get { return _database.Tasks.AsNoTracking().ToList().Select(ToITask).ToList(); } }

        /// <summary>
        /// Get's all servers
        /// </summary>
        public ICollection<VMServer> Servers { get { return _database.Servers.AsNoTracking().ToList().Select(ToVMServer).ToList(); } }

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
        /// Get's all servers (cached)
        /// </summary>
        public ICollection<VMServer> CachedServers { get; private set; }

        /// <summary>
        /// Creates new instance of the SqlCEAccountController
        /// </summary>
        public SqlCEAccountController()
        {
            CachedAccounts = new List<IAccount>(Accounts);
            CachedMachines = new List<IMachine>(Machines);
            CachedTasks = new List<IScheduleTask>(Tasks);
            CachedServers = new List<VMServer>(Servers);
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
                _database.Accounts.Add(ToAccountModel(account));
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
                if (machine is VCenterMachine) _database.VCenterMachines.Add(ToVCenterMachineModel((VCenterMachine)machine));
                else if (machine is HyperVMachine) _database.HyperVMachines.Add(ToHyperVMachineModel((HyperVMachine)machine));

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
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                throw new AddExistingTaskException(task);
            }

            Refresh();
        }

        /// <summary>
        /// Add's task to the database
        /// </summary>
        /// <param name="server">HyperV Server to add</param>
        /// <exception cref="NoSuchAccountException">If <paramref name="server"/>'s owner wasn't found</exception>
        /// <exception cref="AddExistingVMServerException">If <paramref name="server"/> with such id is already presented in the database</exception>
        public void AddVMServer(VMServer server)
        {
            if (!CachedAccounts.Any(x => x.Id == server.Owner)) throw new NoSuchAccountException(server.Owner);
            
            try
            {
                _database.Servers.Add(ToServerModel(server));
                _database.SaveChanges();

                Refresh();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                throw new AddExistingVMServerException(server);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException)
            {
            }

            Refresh();
        }

        /// <summary>
        /// Edit's VM Server with such id
        /// </summary>
        /// <param name="id">Id of the server to edit</param>
        /// <param name="server">New server info (new id and owner doesn't take effect)</param>
        /// <exception cref="NoSuchVMServerException">If server with <paramref name="id"/> wasn't found</exception>
        public void EditVMServer(Guid id, VMServer server)
        {
            var idString = id.ToString();
            var serverInDatabase = _database.Servers.FirstOrDefault(x => x.Id == idString);

            if (serverInDatabase == null) throw new NoSuchVMServerException(id);

            serverInDatabase.ServerName = server.ServerName;
            serverInDatabase.ServerUsername = server.ServerUsername;
            serverInDatabase.ServerPassword = server.ServerPassword;

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Remove's VM server with such id
        /// </summary>
        /// <param name="id">Id of the server to remove</param>
        /// <exception cref="NoSuchVMServerException">If server with <paramref name="id"/> wasn't found</exception>
        public void RemoveVMServer(Guid id)
        {
            var idString = id.ToString();
            var serverInDatabase = _database.Servers.FirstOrDefault(x => x.Id == idString);

            if (serverInDatabase == null) throw new NoSuchVMServerException(id);

            _database.Servers.Remove(serverInDatabase);

            _database.SaveChanges();

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
            var idString = id.ToString(); // Entity doesn't work with tostring method in lamda expression

            var vcenterMachineInDatabase = _database.VCenterMachines.FirstOrDefault(x => x.Id == idString);

            if (vcenterMachineInDatabase == null)
            {
                var hypervMachineInDatabase = _database.HyperVMachines.FirstOrDefault(x => x.Id == idString);
                
                if (hypervMachineInDatabase == null) throw new NoSuchMachineException(id);

                var hypervMachine = machine as HyperVMachine;

                if (hypervMachine == null) throw new InvalidOperationException("Wrong type of the machine");

                hypervMachineInDatabase.Name = hypervMachine.Name;
                hypervMachineInDatabase.Host = hypervMachine.Host;
                hypervMachineInDatabase.UserName = hypervMachine.UserName;
                hypervMachineInDatabase.Password = hypervMachine.Password;
                hypervMachineInDatabase.Share = hypervMachine.Share;
                hypervMachineInDatabase.Address = hypervMachine.Address;
                hypervMachineInDatabase.Snapshot = hypervMachine.Snapshot;
                hypervMachineInDatabase.VirtualMachineName = hypervMachine.VirtualMachineName;
                hypervMachineInDatabase.Server = hypervMachine.Server.ToString();
                hypervMachineInDatabase.Type = machine.Type.ToString();
            }
            else
            {
                var vcenterMachine = machine as VCenterMachine;

                if (vcenterMachine == null) throw new InvalidOperationException("Wrong type of the machine");

                vcenterMachineInDatabase.Name = vcenterMachine.Name;
                vcenterMachineInDatabase.Host = vcenterMachine.Host;
                vcenterMachineInDatabase.UserName = vcenterMachine.UserName;
                vcenterMachineInDatabase.Password = vcenterMachine.Password;
                vcenterMachineInDatabase.Share = vcenterMachine.Share;
                vcenterMachineInDatabase.Address = vcenterMachine.Address;
                vcenterMachineInDatabase.TemplateInventoryPath = vcenterMachine.TemplateInventoryPath;
                vcenterMachineInDatabase.TemplateVMName = vcenterMachine.TemplateVMName;
                vcenterMachineInDatabase.VirtualMachineVMName = vcenterMachine.VirtualMachineVMName;
                vcenterMachineInDatabase.VirtualMachineResourcePool = vcenterMachine.VirtualMachineResourcePool;
                vcenterMachineInDatabase.VirtualMachineInventoryPath = vcenterMachine.VirtualMachineInventoryPath;
                vcenterMachineInDatabase.VirtualMachineDatastore = vcenterMachine.VirtualMachineDatastore;
                vcenterMachineInDatabase.Type = machine.Type.ToString();
                vcenterMachineInDatabase.Server = machine.Server.ToString();
            }

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
            var vcenterMachineInDatabase = _database.VCenterMachines.FirstOrDefault(x => x.Id == idString);

            if (vcenterMachineInDatabase == null)
            {
                var hyperVMachineInDatabase = _database.HyperVMachines.FirstOrDefault(x => x.Id == idString);

                if (hyperVMachineInDatabase == null) throw new NoSuchMachineException(id);

                _database.HyperVMachines.Remove(hyperVMachineInDatabase);
            }
            else
            {
                _database.VCenterMachines.Remove(vcenterMachineInDatabase);
            }

            _database.SaveChanges();

            Refresh();
        }

        /// <summary>
        /// Edit's task with such id
        /// </summary>
        /// <param name="id">Id of the task to edit</param>
        /// <param name="task">New task info (new id and owner doesn't take effect)</param>
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
        /// <exception cref="NoSuchTaskException">If server with <paramref name="id"/> wasn't found</exception>
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
            RefreshSevers();
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

        private void RefreshSevers()
        {
            lock (CachedServers)
            {
                CachedServers = new List<VMServer>(Servers);
            }
        }

        private static AccountModel ToAccountModel(IAccount account)
        {
            return new AccountModel
                {
                    Id = account.Id.ToString(),
                    Login = account.Login,
                    PasswordHash = account.PasswordHash,
                    Type = account.Type.ToString(),
                    VCenterMachines = account.Machines != null ? (ICollection<VCenterMachineModel>)account.Machines.Where(x => x is VCenterMachine).Select(x => ToVCenterMachineModel((VCenterMachine)x)).ToList() : new Collection<VCenterMachineModel>(),
                    HyperVMachines = account.Machines != null ? (ICollection<HyperVMachineModel>)account.Machines.Where(x => x is HyperVMachine).Select(x => ToHyperVMachineModel((HyperVMachine)x)).ToList() : new Collection<HyperVMachineModel>(),
                    Tasks = account.Tasks != null ? (ICollection<TaskModel>) account.Tasks.Select(ToTask).ToList() : new Collection<TaskModel>(),
                    Servers = account.VMServers != null ? (ICollection<ServerModel>)account.VMServers.Select(ToServerModel).ToList() : new Collection<ServerModel>()
                };
        }

        private static VCenterMachineModel ToVCenterMachineModel(VCenterMachine machine)
        {
            return new VCenterMachineModel
                {
                    Id = machine.Id.ToString(),
                    Owner = machine.Owner.ToString(),
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
                    Type = machine.Type.ToString(),
                    Server = machine.Server.ToString()
                };
        }

        private static HyperVMachineModel ToHyperVMachineModel(HyperVMachine machine)
        {
            return new HyperVMachineModel
            {
                Id = machine.Id.ToString(),
                Owner = machine.Owner.ToString(),
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                Snapshot = machine.Snapshot,
                VirtualMachineName = machine.VirtualMachineName,
                Server = machine.Server.ToString(),
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
                Type = (AccountType)Enum.Parse(typeof(AccountType), account.Type, true),
                Machines = account.VCenterMachines.Select(ToIMachine).Union(account.HyperVMachines.Select(ToIMachine)).ToList(),
                Tasks = account.Tasks.Select(ToITask).ToList(),
                VMServers = account.Servers.Select(ToVMServer).ToList()
            };
        }

        private static IMachine ToIMachine(VCenterMachineModel machine)
        {
            return new VCenterMachine
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
                Type = (MachineType)Enum.Parse(typeof(MachineType), machine.Type, true),
                Server = new Guid(machine.Server)
            };
        }

        private static IMachine ToIMachine(HyperVMachineModel machine)
        {
            return new HyperVMachine
            {
                Id = new Guid(machine.Id),
                Owner = new Guid(machine.Owner),
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                Snapshot = machine.Snapshot,
                VirtualMachineName = machine.VirtualMachineName,
                Type = (MachineType)Enum.Parse(typeof(MachineType), machine.Type, true),
                Server = new Guid(machine.Server)
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

        private static VMServer ToVMServer(ServerModel model)
        {
            return new VMServer
                {
                    Id = new Guid(model.Id),
                    ServerName = model.ServerName,
                    ServerPassword = model.ServerPassword,
                    ServerUsername = model.ServerUsername,
                    Owner = new Guid(model.Owner),
                    Type = (VMServerType)Enum.Parse(typeof(VMServerType), model.Type, true),
                };
        }

        private static ServerModel ToServerModel(VMServer model)
        {
            return new ServerModel
                {
                    Id = model.Id.ToString(),
                    ServerName = model.ServerName,
                    ServerPassword = model.ServerPassword,
                    ServerUsername = model.ServerUsername,
                    Owner = model.Owner.ToString(),
                    Type = model.Type.ToString()
                };
        }
    }
}
