using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunWorkflows
{
    public class WorkflowModel
    {
        public int OrderNum { get; set; }
        public int WorkflowItemID { get; set; }
    }

    public class WorkflowResult
    {
        public Boolean Success { get; set; }
        public String ResultString { get; set; }
        public Exception FullError { get; set; }
    }
}
