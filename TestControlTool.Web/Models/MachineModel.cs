using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BootstrapSupport;
using TestControlTool.Core.Contracts;
using TestControlTool.Web.BootstrapSupport;

namespace TestControlTool.Web.Models
{
    public abstract class MachineModel
    {
        public static readonly string[] ShownProperties = new[] { "Name", "Type", "Address", "DestinationType" };

        public static readonly string[] NotShownProperties = new[] { "Id", "Owner", "Server", "DestinationType" };

        [HiddenInput(DisplayValue = false)]
        public Guid Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid Owner { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid Server { get; set; }
        
        [Required]
        [Link(Action = "Edit", Controller = "Machine", Title = "Edit Machine")]
        [Remote("ValidateUniqueMachineName", "Validation", ErrorMessage = "Such name is already used", AdditionalFields = "Id")]
        [Help(Title = "Name", Message = "Provide some machine's name for identification")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Link(Action = "Edit", Controller = "Machine", Title = "Edit Machine")]
        [Display(Name = "Machine Type")]
        public MachineType Type { get; set; }

        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "Address doen't match IPv4 standart")]
        [Link(Action = "Edit", Controller = "Machine", Title = "Edit Machine")]
        [Remote("ValidateUniqueMachineAddress", "Validation", ErrorMessage = "Such address is already in use", AdditionalFields = "Id")]
        [Help(Title = "IP Address", Message = "Please specify IP Address of the machine")]
        [Display(Name = "IP Address")]
        public string Address { get; set; }

        [Link(Action = "Edit", Controller = "Machine", Title = "Edit Machine")]
        [Display(Name = "Deploy on")]
        public List<SelectListItem> DeployOn { get; set; }

        [Display(Name = "Host Name")]
        [Remote("ValidateUniqueMachineHost", "Validation", ErrorMessage = "Such host is already in use", AdditionalFields = "Id")]
        [Help(Title = "Host", Message = "Machine's host. Not required")]
        public string Host { get; set; }

        [Required]
        [Display(Name = "Machine's User name")]
        [Help(Title = "Machine's User name", Message = "User name to logon")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Machine's User password")]
        [Help(Title = "Machine's User name", Message = "User password to logon")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Share folder")]
        [Help(Title = "Share folder", Message = "Share folder on the machine. For example, 'reminst', 'share' etc.")]
        public string Share { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Deploy On")]
        [Link(Action = "Edit", Controller = "Machine", Title = "Edit Machine")]
        public abstract string DestinationType { get; }
    }
}