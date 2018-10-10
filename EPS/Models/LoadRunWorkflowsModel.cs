using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPS.Models
{
    public class LoadRunWorkflowsModel
    {
        public LoadRunWorkflowsModel()
        {
            WorkflowItems = new List<LoadRunWorkflow_WorkflowItems>();
        }

        public int WorkflowID { get; set; }
        public String WorkflowName { get; set; }

        public List<LoadRunWorkflow_WorkflowItems> WorkflowItems { get; set; }
    }

    public class LoadRunWorkflow_WorkflowItems
    {
        public int RunOrder { get; set; }
        public Boolean Disabled { get; set; }
        public int WFItemID { get; set; }
        public int ItemID { get; set; }
        public LoadRunWorkflow_LibraryItem LibraryItem { get; set; }
    }

    public class LoadRunWorkflow_LibraryItem
    {
        public Boolean Disabled { get; set; }
        public int ItemID { get; set; }
        public String ItemName { get; set; }
        public String ItemDesc { get; set; }
        public String HtmlOptions { get; set; }
    }
}