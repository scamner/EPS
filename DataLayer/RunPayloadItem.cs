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
    
    public partial class RunPayloadItem
    {
        public int RunPLItemID { get; set; }
        public int PayloadID { get; set; }
        public int ItemID { get; set; }
        public string ElementID { get; set; }
        public string ElementValue { get; set; }
    
        public virtual LibraryItem LibraryItem { get; set; }
        public virtual RunPayload RunPayload { get; set; }
    }
}