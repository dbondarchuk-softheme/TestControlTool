using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Implementations;
using TestControlTool.Web.Models;

namespace TestControlTool.Web
{
    public static class Extensions
    {
        public static MachineModel ToModel(this IMachine machine)
        {
            if (machine is VCenterMachine)
            {
                return ((VCenterMachine) machine).ToModel();
            }
            else if (machine is HyperVMachine)
            {
                return ((HyperVMachine)machine).ToModel();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static IMachine ToEntity(this MachineModel machine)
        {
            if (machine is VCenterMachineModel)
            {
                return ((VCenterMachineModel)machine).ToEntity();
            }
            else if (machine is HyperVMachineModel)
            {
                return ((HyperVMachineModel)machine).ToEntity();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static VCenterMachineModel ToModel(this VCenterMachine machine)
        {
            return new VCenterMachineModel
            {
                Owner = machine.Owner,
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                Id = machine.Id,
                TemplateInventoryPath = machine.TemplateInventoryPath,
                TemplateVMName = machine.TemplateVMName,
                VirtualMachineVMName = machine.VirtualMachineVMName,
                VirtualMachineResourcePool = machine.VirtualMachineResourcePool,
                VirtualMachineInventoryPath = machine.VirtualMachineInventoryPath,
                VirtualMachineDatastore = machine.VirtualMachineDatastore,
                Server = machine.Server,
                Type = machine.Type
            };
        }

        public static HyperVMachineModel ToModel(this HyperVMachine machine)
        {
            return new HyperVMachineModel
            {
                Owner = machine.Owner,
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                Id = machine.Id,
                Type = machine.Type,
                Server = machine.Server,
                VirtualMachineName = machine.VirtualMachineName,
                Snapshot = machine.Snapshot
            };
        }

        public static VCenterMachine ToEntity(this VCenterMachineModel machine)
        {
            return new VCenterMachine
            {
                Owner = machine.Owner,
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                Server = machine.Server,
                Id = machine.Id,
                TemplateInventoryPath = machine.TemplateInventoryPath,
                TemplateVMName = machine.TemplateVMName,
                VirtualMachineVMName = machine.VirtualMachineVMName,
                VirtualMachineResourcePool = machine.VirtualMachineResourcePool,
                VirtualMachineInventoryPath = machine.VirtualMachineInventoryPath,
                VirtualMachineDatastore = machine.VirtualMachineDatastore,
                Type = machine.Type
            };
        }

        public static HyperVMachine ToEntity(this HyperVMachineModel machine)
        {
            return new HyperVMachine
            {
                Owner = machine.Owner,
                Name = machine.Name,
                Host = machine.Host,
                Address = machine.Address,
                UserName = machine.UserName,
                Password = machine.Password,
                Share = machine.Share,
                Id = machine.Id,
                Type = machine.Type,
                Server = machine.Server,
                VirtualMachineName = machine.VirtualMachineName,
                Snapshot = machine.Snapshot
            };
        }
        
        public static VMServer ToEntity(this ServerModel server)
        {
            return new VMServer
            {
                Owner = server.Owner,
                Id = server.Id,
                Type = server.Type,
                ServerName = server.ServerName,
                ServerUsername = server.ServerUsername,
                ServerPassword = server.ServerPassword
            };
        }

        public static ServerModel ToModel(this VMServer server)
        {
            return new ServerModel
            {
                Owner = server.Owner,
                Id = server.Id,
                Type = server.Type,
                ServerName = server.ServerName,
                ServerUsername = server.ServerUsername,
                ServerPassword = server.ServerPassword
            };
        }

        public static TaskModel ToModel(this IScheduleTask task)
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

        public static IScheduleTask ToEntitiy(this TaskModel task)
        {
            return new ScheduleTask
            {
                Id = task.Id,
                Name = task.Name,
                StartTime = task.StartTime,
                EndTime = task.EndTime,
                Frequency = task.Frequency,
                IsEnabled = task.IsEnabled,
                Owner = task.Owner,
                Status = task.Status,
                LastRun = task.LastRun
            };
        }

        public static List<SelectListItem> ToSelectList<T>(this IEnumerable<T> enumerable, Func<T, string> text, Func<T, string> value, Predicate<T> selectedItem = null, string defaultOption = null)
        {
            var items = enumerable.Select(f => new SelectListItem()
                {
                    Text = text(f),
                    Value = value(f),
                    Selected = selectedItem != null && f.Equals(selectedItem(f))
                }).ToList();

            if (defaultOption != null) items.Insert(0, new SelectListItem()
                {
                    Text = defaultOption,
                    Value = "-1"
                });

            return items;
        }
    }
}