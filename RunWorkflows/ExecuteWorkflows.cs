using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RunWorkflows
{
    public class ExecuteWorkflows
    {
        DataLayer.EPSEntities db = new DataLayer.EPSEntities();
        Utilities util = new Utilities();

        public int RunID { get; set; }

        public void RunWorkflow()
        {
            WorkflowResult finalResult = new WorkflowResult();
            String LastItemRun = "";
            int LastRunResultID = 0;

            try
            {
                RunWorkflow run = db.RunWorkflows.Where(r => r.RunID == RunID).FirstOrDefault();
                List<vwRunItem> items = db.vwRunItems.Where(i => i.RunID == RunID).OrderBy(i => i.RunOrder).ToList();
                RunPayload pl = run.RunPayloads.FirstOrDefault();
                
                foreach (vwRunItem item in items)
                {
                    DateTime StartTime = DateTime.Now;
                    LibraryItem li = db.LibraryItems.Where(l => l.ItemID == item.ItemID).FirstOrDefault();

                    if (!String.IsNullOrEmpty(item.HtmlAnswers))
                    {
                        pl = util.SetPayload(RunID, li.ItemID, item.HtmlAnswers);
                    }

                    LastItemRun = li.ItemName;

                    WorkflowItem wfi = db.WorkflowItems.Where(w => w.ItemID == item.ItemID).FirstOrDefault();

                    RunResult rr = new RunResult
                    {
                        ResultString = "",
                        RunID = RunID,
                        ResultID = 1,
                        TimeStarted = DateTime.Now,
                        WFItemID = wfi.WFItemID
                    };

                    db.RunResults.Add(rr);
                    db.SaveChanges();

                    LastRunResultID = rr.RunResultID;

                    Assembly assembly = Assembly.LoadFrom(li.LibraryPath);
                    Type type = assembly.GetType("ItemToRun.RunWorkflowItem");
                    object instanceOfMyType = Activator.CreateInstance(type);

                    var result = type.InvokeMember("RunItem",
                      BindingFlags.Default | BindingFlags.InvokeMethod,
                      null,
                      instanceOfMyType,
                      new Object[] { run.EmpID, pl == null ? new RunPayload() : pl });

                    IEnumerable<PropertyInfo> props = result.GetType().GetRuntimeProperties();

                    int ResultID = Convert.ToInt32(props.ElementAt(0).GetValue(result, null));
                    String ResultText = props.ElementAt(1).GetValue(result, null).ToString();
                    DateTime TimeDone = Convert.ToDateTime(props.ElementAt(2).GetValue(result, null));

                    RunResult rrDone = db.RunResults.Where(r => r.RunResultID == rr.RunResultID).FirstOrDefault();

                    rrDone.TimeCompleted = DateTime.Now;
                    rrDone.ResultID = ResultID;
                    rrDone.ResultString = ResultText;
                    db.SaveChanges();

                    finalResult = new WorkflowResult { Success = true, ResultString = "" };
                }
            }
            catch (Exception ex)
            {
                finalResult = new WorkflowResult { Success = false, ResultString = String.Format("There was an error during the run in the {0} item.", LastItemRun), FullError = ex };

                RunResult rrError = db.RunResults.Where(r => r.RunResultID == LastRunResultID).FirstOrDefault();
                rrError.ResultID = 4;
                rrError.ResultString = String.Format("Error: {0}", ex.Message);
                rrError.TimeCompleted = DateTime.Now;
                db.SaveChanges();
            }
        }
    }
}
