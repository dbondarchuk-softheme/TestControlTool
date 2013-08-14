using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TestControlTool.Core;
using TestControlTool.Core.Implementations;
using TestControlTool.Web.BootstrapSupport;

namespace TestControlTool.Web.Models
{
    [DisplayName("HyperV Machine")]
    public class HyperVMachineModel : MachineModel
    {
        /*public override string DeployOn
        {
            get { throw new NotImplementedException(); }
        }*/

        [Required]
        [Display(Name = "VM Name")]
        [Help(Title = "VM Name", Message = "Machine's name on the HyperV server, i.e. 'AutomationTesting-Environment3-Core-w7-64-sql8r2-1107'")]
        public string VirtualMachineName { get; set; }

        [Required]
        [Display(Name = "Last snapshot")]
        [Help(Title = "Last snapshot", Message = "Snapshot, from which machine will be reverted, i.e. 'prepared'")]
        public string Snapshot { get; set; }

        public HyperVMachineModel()
        {
            Id = Guid.NewGuid();
        }

        public override string DestinationType
        {
            get { return "HyperV"; }
        }
    }
}