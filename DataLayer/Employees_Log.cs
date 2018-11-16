//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Employees_Log
    {
        public int LogID { get; set; }
        public Nullable<int> EmpID { get; set; }
        public string Username { get; set; }
        public Nullable<System.Guid> ADGUID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Nullable<bool> IsManager { get; set; }
        public Nullable<int> ReportsTo { get; set; }
        public string EmpNum { get; set; }
        public string ChangeType { get; set; }
        public System.DateTime ChangeDate { get; set; }
        public int ChangedBy { get; set; }
    
        public virtual User User { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
