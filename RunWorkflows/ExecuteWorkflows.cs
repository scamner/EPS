using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RunWorkflows
{
    public class ExecuteWorkflows
    {
        DataLayer.EPSEntities db = new DataLayer.EPSEntities();

        public WorkflowResult RunWorkflow(int RunID)
        {
            WorkflowResult finalResult = new WorkflowResult();
            String LastItemRun = "";

            try
            {
                RunWorkflow run = db.RunWorkflows.Where(r => r.RunID == RunID).FirstOrDefault();
                List<vwRunItem> items = db.vwRunItems.Where(i => i.RunID == RunID).OrderBy(i => i.RunOrder).ToList();
                RunPayload pl = run.RunPayloads.FirstOrDefault();

                foreach (vwRunItem item in items)
                {                    
                    DateTime StartTime = DateTime.Now;
                    LibraryItem li = db.LibraryItems.Where(l => l.ItemID == item.ItemID).FirstOrDefault();

                    LastItemRun = li.ItemName;

                    WorkflowItem wfi = db.WorkflowItems.Where(w => w.ItemID == item.ItemID).FirstOrDefault();

                    Assembly assembly = Assembly.LoadFrom(li.LibraryPath);
                    Type type = assembly.GetType("ItemToRun.RunWorkflowItem");
                    object instanceOfMyType = Activator.CreateInstance(type);

                    var result = type.InvokeMember("RunItem",
                      BindingFlags.Default | BindingFlags.InvokeMethod,
                      null,
                      instanceOfMyType,
                      new Object[] { 1, db.Database.Connection.ConnectionString, Newtonsoft.Json.JsonConvert.SerializeObject(pl) });

                    IEnumerable<PropertyInfo> props = result.GetType().GetRuntimeProperties();

                    int ResultID = Convert.ToInt32(props.ElementAt(0).GetValue(result, null));
                    String ResultText = props.ElementAt(1).GetValue(result, null).ToString();
                    DateTime TimeDone = Convert.ToDateTime(props.ElementAt(2).GetValue(result, null));
                    String RunPayload = props.ElementAt(3).GetValue(result, null).ToString();

                    if (!String.IsNullOrEmpty(RunPayload))
                    {
                        if (pl == null)
                        {
                            RunPayload plAdd = new RunPayload { RunID = RunID, RunPayload1 = RunPayload };
                            db.RunPayloads.Add(plAdd);
                            db.SaveChanges();
                        }
                        else
                        {
                            pl.RunPayload1 = RunPayload;
                            db.SaveChanges();
                        }
                    }

                    RunResult rr = new RunResult
                    {
                        ResultString = ResultText,
                        RunID = RunID,
                        ResultID = ResultID,
                        TimeCompleted = TimeDone,
                        TimeStarted = StartTime,
                        WFItemID = wfi.WFItemID
                    };

                    db.RunResults.Add(rr);
                    db.SaveChanges();

                    finalResult = new WorkflowResult { Success = true, ResultString = "" };
                }

                return finalResult;
            }
            catch (Exception ex)
            {
                finalResult = new WorkflowResult { Success = false, ResultString = String.Format("There was an error during the run in the {0} item.", LastItemRun), FullError = ex };
                return finalResult;
            }
        }
    }
}
