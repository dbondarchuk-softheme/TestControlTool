using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BootstrapSupport;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Web.Models
{
    public class MachineModel : IMachine
    {
        public static readonly string[] ShownProperties = new[] { "Name", "Type", "Address" };

        public static readonly string[] NotShownProperties = new[] { "Id", "Owner", "Account" };

        [HiddenInput(DisplayValue = false)]
        public Guid Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid Owner { get; set; }

        [Required]
        [Link(Action = "EditMachine", Controller = "Home", Title = "Edit Machine")]
        [Remote("ValidateUniqueMachineName", "Validation", ErrorMessage = "Such name is already used", AdditionalFields = "Id")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Link(Action = "EditMachine", Controller = "Home", Title = "Edit Machine")]
        [Display(Name = "Machine Type")]
        public MachineType Type { get; set; }
        
        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "Address doen't match IPv4 standart")]
        [Link(Action = "EditMachine", Controller = "Home", Title = "Edit Machine")]
        [Remote("ValidateUniqueMachineAddress", "Validation", ErrorMessage = "Such address is already in use", AdditionalFields = "Id")]
        [Display(Name = "IP Address")]
        public string Address { get; set; }

        [Display(Name = "Host Name")]
        [Remote("ValidateUniqueMachineHost", "Validation", ErrorMessage = "Such host is already in use", AdditionalFields = "Id")]
        public string Host { get; set; }

        [Required]
        [Display(Name = "Machine's User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Machine's User password")]
        public string Password { get; set; }

        [Display(Name = "Share folder")]
        public string Share { get; set; }

        [Required]
        [Remote("ValidateUniqueMachineTemplateVMName", "Validation", ErrorMessage = "Such template name is already in use", AdditionalFields = "Id")]
        [Display(Name = "Template VM Name")]
        public string TemplateVMName { get; set; }

        [Required]
        [Display(Name = "Template InventoryPath")]
        public string TemplateInventoryPath { get; set; }

        [Required]
        [Display(Name = "Virtual Machine Name")]
        public string VirtualMachineVMName { get; set; }

        [Required]
        [Display(Name = "Virtual Machine InventoryPath")]
        public string VirtualMachineInventoryPath { get; set; }

        [Required]
        [Display(Name = "Virtual Machine ResourcePool")]
        public string VirtualMachineResourcePool { get; set; }

        [Required]
        [Display(Name = "Virtual Machine Datastore")]
        public string VirtualMachineDatastore { get; set; }

        public MachineModel()
        {
            Id = Guid.NewGuid();
        }

        public static MachineModel FromIMachine(IMachine machine)
        {
            return new MachineModel
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
                    Type = machine.Type
                };
        }
    }
}