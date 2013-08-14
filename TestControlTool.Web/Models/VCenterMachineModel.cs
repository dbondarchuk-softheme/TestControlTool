using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BootstrapSupport;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Web.BootstrapSupport;

namespace TestControlTool.Web.Models
{
    [DisplayName("VCenter Machine")]
    public class VCenterMachineModel : MachineModel
    {
        public static readonly string[] ShownProperties = new[] { "Name", "Type", "Address" };

        public static readonly string[] NotShownProperties = new[] { "Id", "Owner", "Account" };

        /*[Link(Action = "EditMachine", Controller = "Home", Title = "Edit Machine")]
        [Display(Name = "Deploy on")]
        public override string DeployOn
        {
            get { return "VSphere"; }
        }*/

        [Required]
        [Display(Name = "Template Name")]
        [Remote("ValidateUniqueMachineTemplateVMName", "Validation", ErrorMessage = "Such template name is already in use", AdditionalFields = "Id")]
        [Help(Title = "Template Name", Message = "Name of the template to deploy")]
        public string TemplateVMName { get; set; }

        [Required]
        [Display(Name = "Template Inventory Path")]
        [Help(Title = "Template Inventory Path", Message = "Template's inventory path, i.e. 'applab/QA-Cluster VMs and vApps/batesting'")]
        public string TemplateInventoryPath { get; set; }

        [Required]
        [Display(Name = "Virtual Machine Name")]
        [Help(Title = "Virtual Machine Name", Message = "Name of the deployed virtual machine, without date and username (will be added automatically), i.e. 'Environment1-Core-w12-64-s12-1097 (10.35.174.111)'")]
        public string VirtualMachineVMName { get; set; }

        [Required]
        [Display(Name = "Virtual Machine Inventory Path")]
        [Help(Title = "Virtual Machine Inventory Path", Message = "Machine's inventory path, i.e. 'applab/QA-Cluster VMs and vApps/batesting'")]
        public string VirtualMachineInventoryPath { get; set; }

        [Required]
        [Display(Name = "Virtual Machine Resource Pool")]
        [Help(Title = "Virtual Machine Resource Pool", Message = "Machine's resource pool, i.e. 'QA-Cluster-QA-Pool'")]
        public string VirtualMachineResourcePool { get; set; }

        [Display(Name = "Virtual Machine Datastore")]
        [Help(Title = "Virtual Machine Datastore", Message = "Live this field empty, if you want deploy machine to the same datastore as a template. Or give a full name of the datastore (i.e. qa-cluster-datastore4)")]
        public string VirtualMachineDatastore { get; set; }

        public VCenterMachineModel()
        {
            Id = Guid.NewGuid();
        }

        public override string DestinationType
        {
            get { return "VCenter"; }
        }
    }
}