//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using TestControlTool.AccountController.Implementations.SqlCEModels.AccountModels;

namespace TestControlTool.AccountController.Implementations.SqlCEModels.AccountModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class AccountModel
    {
        public AccountModel()
        {
            this.Machines = new HashSet<MachineModel>();
        }
    
        public string Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
    
        public ICollection<MachineModel> Machines { get; set; }
    }
}
