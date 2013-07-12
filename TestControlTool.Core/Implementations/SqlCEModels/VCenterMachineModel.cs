//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestControlTool.Core.Implementations.SqlCEModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class VCenterMachineModel
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Share { get; set; }
        public string TemplateVMName { get; set; }
        public string TemplateInventoryPath { get; set; }
        public string VirtualMachineVMName { get; set; }
        public string VirtualMachineInventoryPath { get; set; }
        public string VirtualMachineResourcePool { get; set; }
        public string VirtualMachineDatastore { get; set; }
        public string Server { get; set; }
    
        public virtual AccountModel Account { get; set; }
        public virtual ServerModel ServerModel { get; set; }
    }
}