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
    
    public partial class vw_RunResults_Log
    {
        public int RunResultID { get; set; }
        public int RunID { get; set; }
        public int WFItemID { get; set; }
        public int ResultID { get; set; }
        public string ResultString { get; set; }
        public System.DateTime TimeStarted { get; set; }
        public System.DateTime TimeCompleted { get; set; }
        public string ResultStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WorkflowName { get; set; }
        public string ItemName { get; set; }
        public string RunByUser { get; set; }
        public int RunByUserID { get; set; }
        public int WorkflowID { get; set; }
    }
}
