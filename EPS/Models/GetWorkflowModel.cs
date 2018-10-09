using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPS.Models
{
    public class GetWorkflowModel : vwRunWorkflow
    {
        public List<RunResult> ResultItems { get; set; }
    }
}