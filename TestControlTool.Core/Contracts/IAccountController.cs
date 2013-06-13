using System;
using System.Collections.Generic;
using TestControlTool.Core.Exceptions;

namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Describes account controller, which allows to manipulate with accounts
    /// </summary>
    public interface IAccountController
    {
        /// <summary>
        /// Get's all accounts
        /// </summary>
        ICollection<IAccount> Accounts { get; }

        /// <summary>
        /// Get's all machines
        /// </summary>
        ICollection<IMachine> Machines { get; }

        /// <summary>
        /// Get's all accounts (cached)
        /// </summary>
        ICollection<IAccount> CachedAccounts { get; }

        /// <summary>
        /// Get's all machines (cached)
        /// </summary>
        ICollection<IMachine> CachedMachines { get; }

        /// <summary>
        /// Get's all tasks
        /// </summary>
        ICollection<IScheduleTask> Tasks { get; }

        /// <summary>
        /// Get's all tasks (cached)
        /// </summary>
        ICollection<IScheduleTask> CachedTasks { get; }

        /// <summary>
        /// Check's if account login and password are valid
        /// </summary>
        /// <param name="account">Account to check</param>
        /// <returns>True - if valid</returns>
        bool IsValidAccount(IAccount account);

        /// <summary>
        /// Adds account to the database
        /// </summary>
        /// <param name="account">Account to add</param>
        /// <exception cref="AddExistingAccountException">If account with such id or login is already presented in the database</exception>
        void AddAccount(IAccount account);
        
        /// <summary>
        /// Add's machine to the database
        /// </summary>
        /// <param name="machine">Machine to add</param>
        /// <exception cref="NoSuchAccountException">If <paramref name="machine"/>'s owner wasn't found</exception>
        /// <exception cref="AddExistingMachineException">If <paramref name="machine"/> with such id is already presented in the database</exception>
        void AddMachine(IMachine machine);

        /// <summary>
        /// Add's task to the database
        /// </summary>
        /// <param name="task">Task to add</param>
        /// <exception cref="NoSuchAccountException">If <paramref name="task"/>'s owner wasn't found</exception>
        /// <exception cref="AddExistingTaskException">If <paramref name="task"/> with such id is already presented in the database</exception>
        void AddTask(IScheduleTask task);
        
        /// <summary>
        /// Changes password for the account
        /// </summary>
        /// <param name="accountId">Account id</param>
        /// <param name="newPassword">New password to set</param>
        void ChangePassword(Guid accountId, string newPassword);

        /// <summary>
        /// Edit's machine with such id
        /// </summary>
        /// <param name="id">Id of the machine to edit</param>
        /// <param name="machine">New machine info (new id doesn't take effect)</param>
        /// <exception cref="NoSuchMachineException">If machine with <paramref name="id"/> wasn't found</exception>
        void EditMachine(Guid id, IMachine machine);

        /// <summary>
        /// Edit's task with such id
        /// </summary>
        /// <param name="id">Id of the task to edit</param>
        /// <param name="task">New task info (new id and machine doesn't take effect)</param>
        /// <exception cref="NoSuchTaskException">If task with <paramref name="id"/> wasn't found</exception>
        void EditTask(Guid id, IScheduleTask task);

        /// <summary>
        /// Remove's machine with such id
        /// </summary>
        /// <param name="id">Id of the machine to remove</param>
        /// <exception cref="NoSuchMachineException">If machine with <paramref name="id"/> wasn't found</exception>
        void RemoveMachine(Guid id);

        /// <summary>
        /// Remove's task with such id
        /// </summary>
        /// <param name="id">Id of the task to remove</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        void RemoveTask(Guid id);

        /// <summary>
        /// Sets status of the task
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="status">Task's status</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        void SetTaskStatus(Guid id, TaskStatus status);

        /// <summary>
        /// Sets task's last run time
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="time">Last run time</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        void SetTaskLastRun(Guid id, DateTime time);

        /// <summary>
        /// Sets task's last run time and status
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="status">Task's status</param>
        /// <param name="time">Last run time</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        void SetTaskStatusAndLastRun(Guid id, TaskStatus status, DateTime time);

        /// <summary>
        /// Sets task's enabled status
        /// </summary>
        /// <param name="id">Task's id</param>
        /// <param name="enabled">Is task enabled or not</param>
        /// <exception cref="NoSuchTaskException">If machine with <paramref name="id"/> wasn't found</exception>
        void SetTaskEnabled(Guid id, bool enabled);

        /// <summary>
        /// Refreshes cached collections.
        /// </summary>
        void Refresh();
    }
}
