using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BootstrapSupport;
using TestControlTool.Core.Implementations;
using TestControlTool.Web.App_Start;

namespace TestControlTool.Web.Models
{
    [DisplayName("VM Server")]
    public class ServerModel
    {
        public static readonly string[] ShownProperties = new[] { "ServerName", "Type", "ServerUsername" };
        
        public static readonly string[] NotShownProperties = new[] { "Id", "Owner" };

        [HiddenInput(DisplayValue = false)]
        public Guid Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Guid Owner { get; set; }

        [Required]
        [ValidCharachters]
        [Link(Action = "Edit", Controller = "Server", Title = "Edit server")]
        [Display(Name = "Server Address")]
        [Remote("ValidateUniqueServerAddress", "Validation", ErrorMessage = "Such address is already in use", AdditionalFields = "Id")]
        public string ServerName { get; set; }

        [Required]
        [Link(Action = "Edit", Controller = "Server", Title = "Edit server")]
        [Display(Name = "Server Type")]
        public VMServerType Type { get; set; }

        [Required]
        [ValidCharachters]
        [Link(Action = "Edit", Controller = "Server", Title = "Edit server")]
        [Display(Name = "Server Username")]
        public string ServerUsername { get; set; }

        [Required]
        [ValidCharachters]
        [Link(Action = "Edit", Controller = "Server", Title = "Edit server")]
        [Display(Name = "Server Password")]
        [DataType(DataType.Password)]
        public string ServerPassword { get; set; }

        public ServerModel()
        {
            Id = Guid.NewGuid();
        }
    }
}