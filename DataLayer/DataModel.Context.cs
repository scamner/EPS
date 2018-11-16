﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataLayer
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EPSEntities : DbContext
    {
        public EPSEntities()
            : base("name=EPSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Employees_Log> Employees_Log { get; set; }
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<LibraryItem> LibraryItems { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
        public virtual DbSet<RunItem> RunItems { get; set; }
        public virtual DbSet<RunResultStatu> RunResultStatus { get; set; }
        public virtual DbSet<RunWorkflow> RunWorkflows { get; set; }
        public virtual DbSet<User_Log> User_Log { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Workflow_Log> Workflow_Log { get; set; }
        public virtual DbSet<WorkflowItem> WorkflowItems { get; set; }
        public virtual DbSet<Workflow> Workflows { get; set; }
        public virtual DbSet<vw_Managers_From_Logs> vw_Managers_From_Logs { get; set; }
        public virtual DbSet<vwRunItem> vwRunItems { get; set; }
        public virtual DbSet<vwRunWorkflow> vwRunWorkflows { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<Parameters_Log> Parameters_Log { get; set; }
        public virtual DbSet<RunPayload> RunPayloads { get; set; }
        public virtual DbSet<RunPayloadItem> RunPayloadItems { get; set; }
        public virtual DbSet<RunResult> RunResults { get; set; }
        public virtual DbSet<vw_RunResults_Log> vw_RunResults_Log { get; set; }
    }
}
