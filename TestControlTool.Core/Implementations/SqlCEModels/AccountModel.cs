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
    
    public partial class AccountModel
    {
        public AccountModel()
        {
            this.HyperVMachines = new HashSet<HyperVMachineModel>();
            this.Servers = new HashSet<ServerModel>();
            this.Tasks = new HashSet<TaskModel>();
            this.VCenterMachines = new HashSet<VCenterMachineModel>();
        }
    
        public string Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Type { get; set; }
    
        public virtual ICollection<HyperVMachineModel> HyperVMachines { get; set; }
        public virtual ICollection<ServerModel> Servers { get; set; }
        public virtual ICollection<TaskModel> Tasks { get; set; }
        public virtual ICollection<VCenterMachineModel> VCenterMachines { get; set; }
    }
}
