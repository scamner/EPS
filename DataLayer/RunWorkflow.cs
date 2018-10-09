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
    
    public partial class RunWorkflow
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RunWorkflow()
        {
            this.RunItems = new HashSet<RunItem>();
            this.RunPayloads = new HashSet<RunPayload>();
        }
    
        public int RunID { get; set; }
        public int EmpID { get; set; }
        public int WorkflowID { get; set; }
        public System.DateTime StartTime { get; set; }
        public int RunByUserID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunItem> RunItems { get; set; }
        public virtual Workflow Workflow { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunPayload> RunPayloads { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual User User { get; set; }
    }
}
