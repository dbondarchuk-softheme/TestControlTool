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
    
    public partial class ServerModel
    {
        public ServerModel()
        {
            this.HyperVMachines = new HashSet<HyperVMachineModel>();
            this.VCenterMachines = new HashSet<VCenterMachineModel>();
        }
    
        public string Id { get; set; }
        public string Owner { get; set; }
        public string ServerName { get; set; }
        public string Type { get; set; }
        public string ServerUsername { get; set; }
        public string ServerPassword { get; set; }
    
        public virtual AccountModel Account { get; set; }
        public virtual ICollection<HyperVMachineModel> HyperVMachines { get; set; }
        public virtual ICollection<VCenterMachineModel> VCenterMachines { get; set; }
    }
}
