﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using TestControlTool.AccountController.Implementations.SqlCEModels.AccountModels;

namespace TestControlTool.AccountController.Implementations.SqlCEModels
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AccountsEntities : DbContext
    {
        public AccountsEntities() : base("name=AccountsEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<MachineModel> Machines { get; set; }
    }
}
