using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunPendingWorkflows
{
    class Program
    {
        static void Main(string[] args)
        {
            EPSEntities db = new EPSEntities();
            RunWorkflows.ExecuteWorkflows exec = new RunWorkflows.ExecuteWorkflows();

            DateTime today = DateTime.Now;
            List<vwRunWorkflow> pending = db.vwRunWorkflows.Where(v => v.RunStatus == "Pending").OrderBy(v => v.RunID).ToList();

            foreach (vwRunWorkflow wf in pending)
            {
                if (wf.RunDate == "" || Convert.ToDateTime(wf.RunDate) <= today)
                {
                    exec.RunID = wf.RunID;
                    exec.RunWorkflow();

                    Console.WriteLine(String.Format("Run ID: {2} was started for {0} {1}.", wf.FirstName, wf.LastName, wf.RunID));
                }
            }

            System.Threading.Thread.Sleep(20000);
            Environment.Exit(0);
        }
    }
}
