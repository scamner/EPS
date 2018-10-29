using DataLayer;
using EPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Web;

namespace EPS.Controllers
{
    public class DashboardController : Controller
    {
        DataLayer.EPSEntities db = new DataLayer.EPSEntities();
        Models.Utilities util = new Models.Utilities();

        public ActionResult Index(String TabToOpen = "")
        {
            ViewBag.TabToOpen = TabToOpen;
            return View();
        }

        public ActionResult GetEmployees(string sortOrder, string SortDirection, int? page, String Username, String FirstName, String LastName, String IsManager, String ManagerID)
        {
            try
            {
                sortOrder = String.IsNullOrEmpty(sortOrder) ? "SignDate" : sortOrder;
                SortDirection = String.IsNullOrEmpty(SortDirection) ? "desc" : SortDirection;
                ViewBag.CurrentSort = sortOrder;

                int pageSize = 20;
                int pageNumber = (page ?? 1);

                int managerID = Convert.ToInt32(ManagerID);
                String isManager = String.IsNullOrEmpty(IsManager) ? "both" : IsManager;

                List<Employee> emps = db.Employees
                    .Where(e => (String.IsNullOrEmpty(Username) || e.Username.StartsWith(Username)) &&
                        (String.IsNullOrEmpty(FirstName) || e.FirstName.StartsWith(FirstName)) &&
                        (String.IsNullOrEmpty(LastName) || e.LastName.StartsWith(LastName)) &&
                        (isManager == "both" || (isManager == "yes" ? e.IsManager == true : e.IsManager == false)) &&
                        (managerID == 0 || e.ReportsTo == managerID))
                    .OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

                List<Employee> managers = db.Employees.Where(e => e.IsManager == true).ToList();
                List<SelectListItem> managerList = new List<SelectListItem>();
                SelectListItem iempty = new SelectListItem { Text = "", Value = "0", Selected = true };

                managerList.Add(iempty);

                foreach (Employee e in managers)
                {
                    SelectListItem i = new SelectListItem();
                    i.Text = String.Format("{0} {1}", e.FirstName, e.LastName);
                    i.Value = e.EmpID.ToString();
                    managerList.Add(i);
                }

                List<SelectListItem> isManagerList = new List<SelectListItem>();
                SelectListItem both = new SelectListItem { Text = "Both", Value = "both", Selected = true };
                SelectListItem yes = new SelectListItem { Text = "Yes", Value = "yes" };
                SelectListItem no = new SelectListItem { Text = "No", Value = "no" };
                isManagerList.Add(both);
                isManagerList.Add(yes);
                isManagerList.Add(no);

                ViewBag.Managers = managerList;
                ViewBag.Username = Username;
                ViewBag.FirstName = FirstName;
                ViewBag.LastName = LastName;
                ViewBag.IsManager = IsManager;
                ViewBag.ManagerID = ManagerID;
                ViewBag.IsManagerList = isManagerList;

                switch (sortOrder)
                {
                    case "Username":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.Username).ToList();
                        else
                            emps = emps.OrderBy(m => m.Username).ToList();
                        break;

                    case "FirstName":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.FirstName).ToList();
                        else
                            emps = emps.OrderBy(m => m.FirstName).ToList();
                        break;

                    case "LastName":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.LastName).ToList();
                        else
                            emps = emps.OrderBy(m => m.LastName).ToList();
                        break;
                }

                ViewBag.SortDirection = SortDirection == "asc" ? "desc" : "asc";

                return PartialView("_GetEmployees", emps.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetEmployees", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetRuns(Boolean FutureRuns = false)
        {
            try
            {
                DateTime checkTime = DateTime.Now.AddMinutes(-5);

                List<vwRunWorkflow> runs = new List<vwRunWorkflow>();

                if (FutureRuns == false)
                {
                    String today = DateTime.Now.Date.ToShortDateString();

                    runs = db.vwRunWorkflows.
                        Where(r => (r.RunStatus == "Pending" || r.StartTime >= checkTime)
                        && (r.RunDate == "" || r.RunDate == today)
                        && r.RunStatus != "Cancelled")
                        .OrderByDescending(e => e.StartTime).ToList();
                }
                else
                {
                    runs = db.vwRunWorkflows.
                        Where(r => (r.RunStatus == "Pending" || r.StartTime >= checkTime)
                        && r.RunStatus != "Cancelled")
                        .OrderByDescending(e => e.StartTime).ToList();
                }

                List<int> RunIDs = runs.Select(r => r.RunID).Distinct().ToList();
                List<RunResult> Results = db.RunResults.Where(r => RunIDs.Contains(r.RunID)).ToList();

                List<GetWorkflowModel> model = new List<GetWorkflowModel>();

                foreach (vwRunWorkflow wf in runs)
                {
                    String statusColor = "color: orange;";

                    if (wf.RunStatus == "Running")
                    {
                        statusColor = "color: green;";
                    }
                    else if (wf.RunStatus == "Completed Successfully")
                    {
                        statusColor = "color: blue;";
                    }
                    else if (wf.RunStatus == "Completed with Errors")
                    {
                        statusColor = "color: red; font-weight: bold;";
                    }
                    else if (wf.RunStatus == "Completed with Messages")
                    {
                        statusColor = "color: red;";
                    }

                    GetWorkflowModel m = new GetWorkflowModel
                    {
                        RunID = wf.RunID,
                        EmpID = wf.EmpID,
                        FirstName = wf.FirstName,
                        ItemCount = wf.ItemCount,
                        LastName = wf.LastName,
                        ResultItems = Results.Where(r => r.RunID == wf.RunID).OrderBy(r => r.TimeCompleted).ToList(),
                        RunStatus = wf.RunStatus,
                        RunStatusColor = statusColor,
                        StartTime = wf.StartTime,
                        Username = wf.Username,
                        WorkflowID = wf.WorkflowID,
                        RunDate = wf.RunDate
                    };

                    model.Add(m);
                }

                ViewBag.FutureRuns = FutureRuns;

                return PartialView("_GetRuns", model);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetRuns", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetSetup()
        {
            try
            {
                List<Workflow> wf = db.Workflows.OrderBy(e => e.WorkflowName).ToList();
                List<LibraryItem> li = db.LibraryItems.Where(l => l.Disabled == false).OrderBy(l => l.ItemName).ToList();

                ViewBag.LibraryItems = li;
                return PartialView("_GetSetup", wf);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetSetup", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetLogs()
        {
            try
            {
                return PartialView("_GetLogs");
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetLogs", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetAdmin()
        {
            try
            {
                List<User> users = db.Users.OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

                AdminModel model = new AdminModel { Users = users };

                return PartialView("_GetAdmin", model);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetAdmin", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult SearchAD()
        {
            return PartialView("_SearchAD", new ADUser());
        }

        public ActionResult SearchADForUsers(String FirstName, String LastName, Boolean ForUser)
        {
            try
            {
                List<ADUser> users = util.SearchAD(FirstName, LastName, ForUser);
                List<Employee> managers = db.Employees.Where(e => e.IsManager == true).ToList();

                List<SelectListItem> managerList = new List<SelectListItem>();
                SelectListItem iempty = new SelectListItem { Text = "", Value = "0", Selected = true };

                managerList.Add(iempty);

                foreach (Employee e in managers)
                {
                    SelectListItem i = new SelectListItem();
                    i.Text = String.Format("{0} {1}", e.FirstName, e.LastName);
                    i.Value = e.EmpID.ToString();
                    managerList.Add(i);
                }

                ViewBag.Managers = managerList;
                ViewBag.ForUser = ForUser;

                return PartialView("_SearchADForUsers", users);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/SearchADForUsers", ex, String.Format("First Name: {0}, Last Name: {1}", FirstName, LastName));

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public JsonResult AddEmployee(String ADGUID, String Email, String FirstName, String LastName, String Username, String IsManager, String ManagerID)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Employee emp = new Employee
                    {
                        ADGUID = new Guid(ADGUID),
                        Email = Email,
                        FirstName = FirstName,
                        EmpNum = "",
                        LastName = LastName,
                        Username = Username,
                        IsManager = Convert.ToBoolean(IsManager)
                    };

                    if (!String.IsNullOrEmpty(ManagerID) && ManagerID != "0")
                    {
                        emp.ReportsTo = Convert.ToInt32(ManagerID);
                    }

                    db.Employees.Add(emp);
                    db.SaveChanges();

                    Employees_Log log = new Employees_Log
                    {
                        ADGUID = new Guid(ADGUID),
                        Email = Email,
                        FirstName = FirstName,
                        EmpNum = "",
                        LastName = LastName,
                        Username = Username,
                        IsManager = Convert.ToBoolean(IsManager),
                        EmpID = emp.EmpID,
                        ChangeDate = DateTime.Now,
                        ChangedBy = util.GetLoggedOnUser().UserID,
                        ChangeType = "Insert"
                    };

                    if (!String.IsNullOrEmpty(ManagerID) && ManagerID != "0")
                    {
                        log.ReportsTo = Convert.ToInt32(ManagerID);
                    }

                    db.Employees_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/AddEmployee", ex, String.Format("First Name: {0}, Last Name: {1}", FirstName, LastName));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult SetEmployeeAsManager(int EmpID, Boolean IsManager)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    int isManagerCount = db.Employees.Where(e => e.ReportsTo == EmpID).Count();

                    if (isManagerCount > 0)
                    {
                        return Json(new { Error = "That employee is assigned as a manager to other employees. Please change that first.", JsonRequestBehavior.AllowGet });
                    }

                    Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
                    Employees_Log logB = new Employees_Log
                    {
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Username = emp.Username,
                        IsManager = emp.IsManager,
                        EmpID = emp.EmpID,
                        ChangeDate = DateTime.Now,
                        ChangedBy = util.GetLoggedOnUser().UserID,
                        ChangeType = "Before Update"
                    };

                    emp.IsManager = IsManager;
                    db.SaveChanges();

                    db.Employees_Log.Add(logB);
                    db.SaveChanges();

                    Employees_Log logA = new Employees_Log
                    {
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Username = emp.Username,
                        IsManager = Convert.ToBoolean(IsManager),
                        EmpID = emp.EmpID,
                        ChangeDate = DateTime.Now,
                        ChangedBy = util.GetLoggedOnUser().UserID,
                        ChangeType = "After Update"
                    };

                    db.Employees_Log.Add(logA);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "", ManagerName = String.Format("{0} {1}", emp.FirstName, emp.LastName), JsonRequestBehavior.AllowGet });
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/SetEmployeeAsManager", ex, String.Format("Emp ID: {0}, Is Manager: {1}", EmpID.ToString(), IsManager.ToString()));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult SetEmployeeReportTo(int EmpID, String ManagerID)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();

                    Employees_Log logB = new Employees_Log
                    {
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Username = emp.Username,
                        ReportsTo = emp.ReportsTo,
                        EmpID = emp.EmpID,
                        ChangeDate = DateTime.Now,
                        ChangedBy = util.GetLoggedOnUser().UserID,
                        ChangeType = "Before Update"
                    };

                    if (Convert.ToInt32(ManagerID) == 0) { emp.ReportsTo = null; }
                    else { emp.ReportsTo = Convert.ToInt32(ManagerID); }
                    db.SaveChanges();

                    db.Employees_Log.Add(logB);
                    db.SaveChanges();

                    Employees_Log logA = new Employees_Log
                    {
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Username = emp.Username,
                        ReportsTo = Convert.ToInt32(ManagerID),
                        EmpID = emp.EmpID,
                        ChangeDate = DateTime.Now,
                        ChangedBy = util.GetLoggedOnUser().UserID,
                        ChangeType = "After Update"
                    };

                    db.Employees_Log.Add(logA);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/SetEmployeeReportTo", ex, String.Format("Emp ID: {0}, Manager ID: {1}", EmpID.ToString(), ManagerID.ToString()));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult LoadRunWorkflows()
        {
            try
            {
                List<Workflow> wf = db.Workflows.Where(w => w.Disabled == false).OrderBy(w => w.WorkflowName).ToList();
                List<LoadRunWorkflowsModel> model = new List<LoadRunWorkflowsModel>();

                foreach (Workflow w in wf)
                {
                    LoadRunWorkflowsModel lr = new LoadRunWorkflowsModel
                    {
                        WorkflowID = w.WorkflowID,
                        WorkflowName = w.WorkflowName
                    };

                    foreach (WorkflowItem i in w.WorkflowItems.OrderBy(wfi => wfi.RunOrder))
                    {
                        LoadRunWorkflow_WorkflowItems wfi = new LoadRunWorkflow_WorkflowItems
                        {
                            Disabled = i.Disabled,
                            ItemID = i.ItemID,
                            RunOrder = i.RunOrder,
                            WFItemID = i.WFItemID
                        };

                        wfi.LibraryItem = new LoadRunWorkflow_LibraryItem
                        {
                            Disabled = i.LibraryItem.Disabled,
                            HtmlOptions = i.LibraryItem.HtmlOptions,
                            ItemDesc = i.LibraryItem.ItemDesc,
                            ItemID = i.LibraryItem.ItemID,
                            ItemName = i.LibraryItem.ItemName
                        };

                        lr.WorkflowItems.Add(wfi);
                    }

                    model.Add(lr);
                }

                var list = JsonConvert.SerializeObject(model, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                });

                return Json(new { Error = "", WorkFlows = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                String error = "There was an error loading your workflows.";
                util.WriteErrorToLog("Dashboard/LoadRunWorkflows", ex, "");

                return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RunWorkflow(int EmpID, int WorkflowID, int[] ItemIDs, String[] HtmlOptions, String RunDate)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User CurrentUser = util.GetLoggedOnUser();

                    RunWorkflow run = new RunWorkflow
                    {
                        EmpID = EmpID,
                        RunByUserID = CurrentUser.UserID,
                        StartTime = DateTime.Now,
                        WorkflowID = WorkflowID
                    };

                    if (!String.IsNullOrEmpty(RunDate))
                    {
                        run.RunDate = Convert.ToDateTime(RunDate);
                    }

                    db.RunWorkflows.Add(run);
                    db.SaveChanges();

                    foreach (int itemID in ItemIDs)
                    {
                        var htmlOption = HtmlOptions.Where(h => h.ToString()
                                .StartsWith(String.Format("{0}:", itemID))).FirstOrDefault();

                        RunItem ri = new RunItem
                        {
                            HtmlAnswers = htmlOption == null ? "" : htmlOption.ToString(),
                            RunID = run.RunID,
                            WFItemID = itemID
                        };

                        db.RunItems.Add(ri);
                        db.SaveChanges();
                    }

                    tran.Commit();

                    RunWorkflows.ExecuteWorkflows exec = new RunWorkflows.ExecuteWorkflows();

                    if (String.IsNullOrEmpty(RunDate) || Convert.ToDateTime(RunDate).Date == DateTime.Now.Date)
                    {
                        var syncTask = new Task<RunWorkflows.WorkflowResult>(() => {
                            RunWorkflows.WorkflowResult res = exec.RunWorkflow(run.RunID);
                            return res;
                        });
                        syncTask.RunSynchronously();

                        if(syncTask.Result.Success == false)
                        {
                            throw syncTask.Result.FullError;
                        }
                    }

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    if (tran.UnderlyingTransaction.Connection != null)
                    {
                        tran.Rollback();
                    }

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/RunWorkflow", ex, String.Format("Emp ID: {0}", EmpID));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult CancelWorkflowRun(int RunID)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User CurrentUser = util.GetLoggedOnUser();

                    RunWorkflow run = db.RunWorkflows.Where(w => w.RunID == RunID).FirstOrDefault();

                    String wfName = run.Workflow.WorkflowName;
                    String fName = run.Employee.FirstName;
                    String lName = run.Employee.LastName;
                    String runDate = run.RunDate == null ? "" : run.RunDate.Value.ToShortDateString();

                    foreach (RunItem ri in run.RunItems)
                    {
                        RunResult rr = new RunResult
                        {
                            ResultString = String.Format("Workflow Run: {0} {1}on user {2} {3} was Cancelled.", wfName, runDate == "" ? "" : "that was scheduled for " + runDate + " ", fName, lName),
                            RunID = RunID,
                            ResultID = 6,
                            TimeCompleted = DateTime.Now,
                            TimeStarted = run.StartTime,
                            WFItemID = Convert.ToInt32(ri.WFItemID)
                        };

                        db.RunResults.Add(rr);
                        db.SaveChanges();
                    }

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    if (tran.UnderlyingTransaction.Connection != null)
                    {
                        tran.Rollback();
                    }

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/CancelWorkflowRun", ex, String.Format("Workflow Run ID: {0}", RunID));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult SetWorkflowDisabled(int WorkflowID, Boolean Disabled)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User CurrentUser = util.GetLoggedOnUser();

                    Workflow wf = db.Workflows.Where(w => w.WorkflowID == WorkflowID).FirstOrDefault();

                    wf.Disabled = Disabled;
                    db.SaveChanges();

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = String.Format("Workflow: {0} was set to {1}", wf.WorkflowName, Disabled == true ? "DISABLED" : "Enabled"),
                        ChangeType = "Update",
                        ItemID = wf.WorkflowID,
                        ItemName = wf.WorkflowName,
                        ItemType = "Workflow"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    if (tran.UnderlyingTransaction.Connection != null)
                    {
                        tran.Rollback();
                    }

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/SetWorkflowDisabled", ex, String.Format("Workflow ID: {0}", WorkflowID, Disabled));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult SetWorkflowItemDisabled(int WFItemID, Boolean Disabled)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User CurrentUser = util.GetLoggedOnUser();

                    WorkflowItem wf = db.WorkflowItems.Where(w => w.WFItemID == WFItemID).FirstOrDefault();
                    wf.Disabled = Disabled;
                    db.SaveChanges();

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = String.Format("Workflow Item: {0} was set to {1} in Workflow: {2}", wf.LibraryItem.ItemName, Disabled == true ? "DISABLED" : "Enabled", wf.Workflow.WorkflowName),
                        ChangeType = "Update",
                        ItemID = wf.WFItemID,
                        ItemName = wf.LibraryItem.ItemName,
                        ItemType = "Workflow Item"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    if (tran.UnderlyingTransaction.Connection != null)
                    {
                        tran.Rollback();
                    }

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/SetWorkflowItemDisabled", ex, String.Format("Workflow Item ID: {0}", WFItemID, Disabled));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ManageWFItems()
        {
            try
            {
                List<LibraryItem> items = db.LibraryItems.OrderBy(l => l.ItemName).ToList();

                return PartialView("_ManageWFItems", items);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/ManageWFItems", ex, "");

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult AddLibraryItem()
        {
            return PartialView("_AddLibraryItem", new NewLibraryItemModel());
        }

        [HttpPost]
        public JsonResult SaveLibraryItem(int ItemID, String ItemName, String ItemDesc, Boolean Disabled, String HtmlOptions)
        {
            User CurrentUser = util.GetLoggedOnUser();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    LibraryItem item = db.LibraryItems.Where(i => i.ItemID == ItemID).FirstOrDefault();

                    StringBuilder sb = new StringBuilder();
                    if (ItemName != item.ItemName) { sb.AppendFormat("Item name changed from {0} to {1}.<br>", item.ItemName, ItemName); }
                    if (ItemDesc != item.ItemDesc) { sb.AppendFormat("Item description changed from {0} to {1}.<br>", item.ItemDesc, ItemDesc); }
                    if (Disabled != item.Disabled) { sb.AppendFormat("Disabled changed from {0} to {1}.<br>", item.Disabled, Disabled); }
                    if (HtmlOptions != item.HtmlOptions) { sb.AppendFormat("Html Options changed from {0} to {1}.<br>", item.HtmlOptions, ItemName); }

                    item.ItemName = ItemName;
                    item.ItemDesc = ItemDesc;
                    item.Disabled = Disabled;
                    item.HtmlOptions = HtmlOptions;

                    db.SaveChanges();

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = sb.ToString(),
                        ChangeType = "Update",
                        ItemID = ItemID,
                        ItemName = ItemName,
                        ItemType = "Library Item"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/SaveLibraryItem", ex, String.Format("Item ID: {0}", ItemID));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult SaveWorkflow(int WorkflowID, String WorkflowName, String WorkflowDesc, Boolean Disabled)
        {
            User CurrentUser = util.GetLoggedOnUser();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Workflow wf = db.Workflows.Where(i => i.WorkflowID == WorkflowID).FirstOrDefault();

                    StringBuilder sb = new StringBuilder();
                    if (WorkflowName != wf.WorkflowName) { sb.AppendFormat("Workflow name changed from {0} to {1}.<br>", wf.WorkflowName, WorkflowName); }
                    if (WorkflowDesc != wf.WorkflowDesc) { sb.AppendFormat("Workflow description changed from {0} to {1}.<br>", wf.WorkflowDesc, WorkflowDesc); }
                    if (Disabled != wf.Disabled) { sb.AppendFormat("Disabled changed from {0} to {1}.<br>", wf.Disabled, Disabled); }

                    wf.WorkflowName = WorkflowName;
                    wf.WorkflowDesc = WorkflowDesc;
                    wf.Disabled = Disabled;

                    db.SaveChanges();

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = sb.ToString(),
                        ChangeType = "Update",
                        ItemID = WorkflowID,
                        ItemName = WorkflowName,
                        ItemType = "Workflow"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/SaveWorkflow", ex, String.Format("Workflow ID: {0}", WorkflowID));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult NewLibraryItem(NewLibraryItemModel model)
        {
            try
            {
                FileInfo fi = new FileInfo(model.LibraryPathFile.FileName);

                if (fi.Extension != ".dll")
                {
                    throw new Exception("You must upload a .dll file.");
                }

                Byte[] file;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    model.LibraryPathFile.InputStream.CopyTo(memoryStream);

                    file = memoryStream.ToArray();
                    memoryStream.Close();
                    memoryStream.Dispose();
                }

                string binPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin");
                string fullPath = String.Format("{0}\\{1}", binPath, fi.Name);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                System.IO.File.WriteAllBytes(fullPath, file);

                LibraryItem li = new LibraryItem
                {
                    Disabled = false,
                    HtmlOptions = model.HtmlOptions == null ? "" : model.HtmlOptions,
                    ItemDesc = model.ItemDesc,
                    ItemName = model.ItemName,
                    LibraryPath = fullPath
                };

                db.LibraryItems.Add(li);
                db.SaveChanges();

                String fullURLRoot = string.Format("{0}://{1}{2}", System.Web.HttpContext.Current.Request.Url.Scheme, System.Web.HttpContext.Current.Request.Url.Authority, VirtualPathUtility.ToAbsolute("~"));

                return Redirect(String.Format("{0}Dashboard?TabToOpen=GetSetup", fullURLRoot));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/NewLibraryItem", ex, "");

                return Content(String.Format("<script type='text/javascript'>alert('{0}');</script>", error));
            }
        }

        public ActionResult AddWorkflow()
        {
            try
            {
                List<LibraryItem> items = db.LibraryItems.Where(l => l.Disabled == false).OrderBy(l => l.ItemName).ToList();

                return PartialView("_AddWorkflow", new AddWorkflowModel { items = items });
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/AddWorkflow", ex, "");

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult NewWorkflow(String WorkflowName, String WorkflowDesc, int[] LibraryItem)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Workflow wf = new Workflow { Disabled = false, WorkflowDesc = WorkflowDesc, WorkflowName = WorkflowName };
                    db.Workflows.Add(wf);
                    db.SaveChanges();

                    StringBuilder sbItemLog = new StringBuilder();

                    if (LibraryItem != null && LibraryItem.Count() > 0)
                    {
                        int count = 1;

                        foreach (int li in LibraryItem)
                        {
                            WorkflowItem wfi = new WorkflowItem
                            {
                                Disabled = false,
                                ItemID = li,
                                RunOrder = count,
                                WorkflowID = wf.WorkflowID
                            };

                            db.WorkflowItems.Add(wfi);
                            db.SaveChanges();

                            LibraryItem liLog = db.LibraryItems.Where(l => l.ItemID == li).FirstOrDefault();

                            sbItemLog.AppendFormat("{0}, ", liLog.ItemName);

                            count += 1;
                        }
                    }

                    User CurrentUser = util.GetLoggedOnUser();
                    String itemLog = sbItemLog.Length > 0 ? sbItemLog.ToString().Remove(sbItemLog.Length - 2, 2) : "NONE";

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = String.Format("Workflow: {0} was added and includes workflow items: {1}.", wf.WorkflowName, itemLog),
                        ChangeType = "Insert",
                        ItemID = wf.WorkflowID,
                        ItemName = wf.WorkflowName,
                        ItemType = "Workflow"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Content("<script type='text/javascript'>WorkflowAdded();</script>");
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/NewWorkflow", ex, String.Format("WorkflowName: {0}", WorkflowName));

                    return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
                }
            }
        }

        [HttpPost]
        public JsonResult AddWorflowItemToWF(int WorkflowID, int ItemID)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User CurrentUser = util.GetLoggedOnUser();
                    LibraryItem li = db.LibraryItems.Where(l => l.ItemID == ItemID).FirstOrDefault();
                    Workflow wf = db.Workflows.Where(w => w.WorkflowID == WorkflowID).FirstOrDefault();
                    int newOrderNum = db.WorkflowItems.Where(w => w.WorkflowID == WorkflowID).Any() == false ? 1 :
                        db.WorkflowItems.Where(w => w.WorkflowID == WorkflowID).OrderByDescending(w => w.RunOrder).FirstOrDefault().RunOrder + 1;

                    WorkflowItem wfi = new WorkflowItem
                    {
                        Disabled = false,
                        ItemID = ItemID,
                        RunOrder = newOrderNum,
                        WorkflowID = WorkflowID
                    };
                    db.WorkflowItems.Add(wfi);
                    db.SaveChanges();

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = String.Format("Added '{1}' to the workflow '{0}'.", wf.WorkflowName, li.ItemName),
                        ChangeType = "Insert",
                        ItemID = WorkflowID,
                        ItemName = li.ItemName,
                        ItemType = "Workflow Item"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/AddWorflowItemToWF", ex, String.Format("Workflow ID: {0}, Item ID: {1}", WorkflowID, ItemID));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult ChangeRunOrder(int WFItemID, String Direction)
        {
            User CurrentUser = util.GetLoggedOnUser();

            WorkflowItem wfi = db.WorkflowItems.Where(w => w.WFItemID == WFItemID).FirstOrDefault();
            WorkflowItem wfiNeighbor = new WorkflowItem();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    if (Direction == "up")
                    {
                        wfiNeighbor = db.WorkflowItems.Where(w => w.WorkflowID == wfi.WorkflowID
                            && w.RunOrder == (wfi.RunOrder - 1)).FirstOrDefault();

                        wfi.RunOrder = wfi.RunOrder - 1;
                        db.SaveChanges();

                        wfiNeighbor.RunOrder = wfiNeighbor.RunOrder + 1;
                        db.SaveChanges();
                    }
                    else
                    {
                        wfiNeighbor = db.WorkflowItems.Where(w => w.WorkflowID == wfi.WorkflowID
                            && w.RunOrder == (wfi.RunOrder + 1)).FirstOrDefault();

                        wfi.RunOrder = wfi.RunOrder + 1;
                        db.SaveChanges();

                        wfiNeighbor.RunOrder = wfiNeighbor.RunOrder - 1;
                        db.SaveChanges();
                    }

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = String.Format("Moved '{0}' {1} in the workflow '{2}'.", wfi.LibraryItem.ItemName, Direction, wfi.Workflow.WorkflowName),
                        ChangeType = "Update",
                        ItemID = WFItemID,
                        ItemName = wfi.LibraryItem.ItemName,
                        ItemType = "Workflow Item"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/ChangeRunOrder", ex, String.Format("WFItemID: {0}, Direction: {1}", WFItemID, Direction));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetUsers()
        {
            try
            {
                List<User> users = db.Users.OrderBy(l => l.FirstName).ToList();

                return PartialView("_GetUsers", users);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/AddWorkflow", ex, "");

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        [HttpPost]
        public JsonResult DeleteUser(int UserID)
        {
            User CurrentUser = util.GetLoggedOnUser();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User user = db.Users.Where(u => u.UserID == UserID).FirstOrDefault();

                    User userLog = user;

                    db.Users.Remove(user);
                    db.SaveChanges();

                    User_Log log = new User_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeType = "Delete",
                        FirstName = userLog.FirstName,
                        LastName = userLog.LastName,
                        UserID = UserID,
                        Username = userLog.Username
                    };

                    db.User_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/DeleteUser", ex, String.Format("UserID: {0}", UserID));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult AddUser(String ADGUID, String Email, String FirstName, String LastName, String Username)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    User user = new User
                    {
                        Email = Email,
                        FirstName = FirstName,
                        LastName = LastName,
                        Username = Username
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    User_Log log = new User_Log
                    {
                        UserID = user.UserID,
                        FirstName = FirstName,
                        LastName = LastName,
                        Username = Username,
                        ChangeDate = DateTime.Now,
                        ChangedBy = util.GetLoggedOnUser().UserID,
                        ChangeType = "Insert"
                    };

                    db.User_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "", UserID = user.UserID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/AddUser", ex, String.Format("First Name: {0}, Last Name: {1}", FirstName, LastName));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetParameters()
        {
            try
            {
                List<Parameter> model = db.Parameters.OrderBy(l => l.ParamName).ToList();

                return PartialView("_GetParameters", model);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/AddWorkflow", ex, "");

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult AddParam()
        {
            return PartialView("_AddParam", new AddParamModel());
        }

        public ActionResult NewParam(String ParamName, String ParamDesc, String EncodedParamValue)
        {
            EncodedParamValue = HttpUtility.UrlDecode(EncodedParamValue);

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Parameter p = new Parameter { ParamName = ParamName, ParamDesc = ParamDesc, ParamValue = EncodedParamValue };
                    db.Parameters.Add(p);
                    db.SaveChanges();

                    User CurrentUser = util.GetLoggedOnUser();

                    Parameters_Log log = new Parameters_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeType = "Insert",
                        ParamDesc = ParamDesc,
                        ParamName = ParamName,
                        ParamValue = EncodedParamValue
                    };

                    db.Parameters_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Content("<script type='text/javascript'>ParamAdded();</script>");
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/NewParam", ex, String.Format("Parameter Name: {0}", ParamName));

                    return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
                }
            }
        }

        [HttpPost]
        public JsonResult EditParam(int ParamID, String ParamName, String ParamDesc, String ParamValue)
        {
            ParamName = HttpUtility.UrlDecode(ParamName);
            ParamDesc = HttpUtility.UrlDecode(ParamDesc);
            ParamValue = HttpUtility.UrlDecode(ParamValue);

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Parameter p = db.Parameters.Where(pa => pa.ParamID == ParamID).FirstOrDefault();

                    String beforeParamDesc = p.ParamDesc;
                    String beforeParamName = p.ParamName;
                    String beforeParamValue = p.ParamValue;

                    p.ParamDesc = ParamDesc;
                    p.ParamName = ParamName;
                    p.ParamValue = ParamValue;
                    db.SaveChanges();

                    User CurrentUser = util.GetLoggedOnUser();

                    Parameters_Log logB = new Parameters_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeType = "Before Update",
                        ParamDesc = beforeParamDesc,
                        ParamName = beforeParamName,
                        ParamValue = beforeParamValue,
                    };

                    db.Parameters_Log.Add(logB);
                    db.SaveChanges();

                    Parameters_Log logA = new Parameters_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeType = "After Update",
                        ParamDesc = ParamDesc,
                        ParamName = ParamName,
                        ParamValue = ParamValue,
                    };

                    db.Parameters_Log.Add(logA);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/EditParam", ex, String.Format("Param Name: {0}", ParamName));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult DeleteParam(int ParamID)
        {
            Parameter logp = new Parameter();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    Parameter p = db.Parameters.Where(pa => pa.ParamID == ParamID).FirstOrDefault();
                    logp = p;

                    db.Parameters.Remove(p);
                    db.SaveChanges();

                    User CurrentUser = util.GetLoggedOnUser();

                    Parameters_Log log = new Parameters_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeType = "Delete",
                        ParamDesc = logp.ParamDesc,
                        ParamName = logp.ParamName,
                        ParamValue = logp.ParamValue
                    };

                    db.Parameters_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/NewParam", ex, String.Format("Parameter Name: {0}", logp.ParamName));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult RemoveItemFromWF(int WFItemID)
        {
            String workflowName = "";
            String libraryItemName = "";

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    WorkflowItem wfi = db.WorkflowItems.Where(w => w.WFItemID == WFItemID).FirstOrDefault();
                    workflowName = wfi.Workflow.WorkflowName;
                    libraryItemName = wfi.LibraryItem.ItemName;

                    db.WorkflowItems.Remove(wfi);
                    db.SaveChanges();

                    User CurrentUser = util.GetLoggedOnUser();

                    Workflow_Log log = new Workflow_Log
                    {
                        ChangeDate = DateTime.Now,
                        ChangedBy = CurrentUser.UserID,
                        ChangeText = String.Format("'{0}' was removed form the workflow '{1}'.", libraryItemName, workflowName),
                        ChangeType = "Delete",
                        ItemID = WFItemID,
                        ItemName = libraryItemName,
                        ItemType = "Workflow Item"
                    };

                    db.Workflow_Log.Add(log);
                    db.SaveChanges();

                    tran.Commit();

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/RemoveItemFromWF", ex, String.Format("Library Item Name: {0}, Workflow Name: {1}", libraryItemName, workflowName));

                    return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetEmployeesLog(string sortOrder, string SortDirection, int? page, String Username, String FirstName, String LastName, int? AuditUser, String AuditDateFrom, String AuditDateTo, String ChangeType)
        {
            try
            {
                sortOrder = String.IsNullOrEmpty(sortOrder) ? "FirstName" : sortOrder;
                SortDirection = String.IsNullOrEmpty(SortDirection) ? "desc" : SortDirection;
                ViewBag.CurrentSort = sortOrder;

                int pageSize = 25;
                int pageNumber = (page ?? 1);

                DateTime? auditDateFrom = String.IsNullOrEmpty(AuditDateFrom) ? new DateTime() : Convert.ToDateTime(AuditDateFrom);
                DateTime? auditDateTo = String.IsNullOrEmpty(AuditDateTo) ? new DateTime() : Convert.ToDateTime(AuditDateTo);

                List<Employees_Log> emps = db.Employees_Log
                    .Where(e => (String.IsNullOrEmpty(Username) || e.Username.StartsWith(Username)) &&
                        (String.IsNullOrEmpty(FirstName) || e.FirstName.StartsWith(FirstName)) &&
                        (String.IsNullOrEmpty(LastName) || e.LastName.StartsWith(LastName)) &&
                        (AuditUser == null || e.ChangedBy == AuditUser) &&
                        (auditDateFrom.Value.Year == 0001 || e.ChangeDate > auditDateFrom) &&
                        (auditDateTo.Value.Year == 0001 || e.ChangeDate < auditDateTo) &&
                        (String.IsNullOrEmpty(ChangeType) || e.ChangeType.Contains(ChangeType))
                        ).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

                ViewBag.Username = Username;
                ViewBag.FirstName = FirstName;
                ViewBag.LastName = LastName;
                ViewBag.AuditUser = AuditUser;
                ViewBag.AuditDateFrom = AuditDateFrom;
                ViewBag.AuditDateTo = AuditDateTo;
                ViewBag.ChangeType = ChangeType;

                switch (sortOrder)
                {
                    case "Username":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.Username).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.Username).ThenBy(m => m.LogID).ToList();
                        break;

                    case "FirstName":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.FirstName).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.FirstName).ThenBy(m => m.LogID).ToList();
                        break;

                    case "LastName":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.LastName).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.LastName).ThenBy(m => m.LogID).ToList();
                        break;

                    case "Email":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.Email).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.Email).ThenBy(m => m.LogID).ToList();
                        break;

                    case "Manager":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.IsManager).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.IsManager).ThenBy(m => m.LogID).ToList();
                        break;

                    case "ReportsTo":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.ReportsTo).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.ReportsTo).ThenBy(m => m.LogID).ToList();
                        break;

                    case "EmpNum":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.EmpNum).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.EmpNum).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditType":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditDate":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditBy":
                        if (SortDirection == "desc")
                            emps = emps.OrderByDescending(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        else
                            emps = emps.OrderBy(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        break;
                }

                List<User> users = db.Users.OrderBy(u => u.FirstName).ToList();
                List<SelectListItem> userList = new List<SelectListItem>();
                SelectListItem iempty = new SelectListItem { Text = "", Value = "", Selected = true };
                userList.Add(iempty);

                foreach (User e in users)
                {
                    SelectListItem i = new SelectListItem();
                    i.Text = String.Format("{0} {1}", e.FirstName, e.LastName);
                    i.Value = e.UserID.ToString();
                    userList.Add(i);
                }

                List<SelectListItem> changeTypes = new List<SelectListItem> {
                    new SelectListItem { Selected = true, Text = "", Value = "" },
                    new SelectListItem { Text = "Insert", Value = "Insert" },
                    new SelectListItem { Text = "Update", Value = "Update" },
                    new SelectListItem { Text = "Delete", Value = "Delete" }
                };

                List<vw_Managers_From_Logs> managers = db.vw_Managers_From_Logs.OrderBy(m => m.ManagerName).ToList();

                ViewBag.Managers = managers;
                ViewBag.ChangeTypes = changeTypes;
                ViewBag.Users = userList;
                ViewBag.SortDirection = SortDirection == "asc" ? "desc" : "asc";

                return PartialView("_GetEmployeesLog", emps.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetEmployeesLog", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetParametersLog(string sortOrder, string SortDirection, int? page, String ParamName, int? AuditUser, String AuditDateFrom, String AuditDateTo, String ChangeType)
        {
            try
            {
                sortOrder = String.IsNullOrEmpty(sortOrder) ? "ParamName" : sortOrder;
                SortDirection = String.IsNullOrEmpty(SortDirection) ? "desc" : SortDirection;
                ViewBag.CurrentSort = sortOrder;

                int pageSize = 25;
                int pageNumber = (page ?? 1);

                DateTime? auditDateFrom = String.IsNullOrEmpty(AuditDateFrom) ? new DateTime() : Convert.ToDateTime(AuditDateFrom);
                DateTime? auditDateTo = String.IsNullOrEmpty(AuditDateTo) ? new DateTime() : Convert.ToDateTime(AuditDateTo);

                List<Parameters_Log> logs = db.Parameters_Log
                    .Where(e => (String.IsNullOrEmpty(ParamName) || e.ParamName.StartsWith(ParamName)) &&
                        (AuditUser == null || e.ChangedBy == AuditUser) &&
                        (auditDateFrom.Value.Year == 0001 || e.ChangeDate > auditDateFrom) &&
                        (auditDateTo.Value.Year == 0001 || e.ChangeDate < auditDateTo) &&
                        (String.IsNullOrEmpty(ChangeType) || e.ChangeType.Contains(ChangeType))
                        ).OrderBy(e => e.ParamName).ToList();

                ViewBag.ParamName = ParamName;
                ViewBag.AuditUser = AuditUser;
                ViewBag.AuditDateFrom = AuditDateFrom;
                ViewBag.AuditDateTo = AuditDateTo;
                ViewBag.ChangeType = ChangeType;

                switch (sortOrder)
                {
                    case "ParamName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ParamName).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ParamName).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditType":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditDate":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditBy":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        break;
                }

                List<Parameter> prm = db.Parameters.OrderBy(u => u.ParamName).ToList();
                List<SelectListItem> paramList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (Parameter e in prm)
                {
                    paramList.Add(new SelectListItem { Text = String.Format("{0}", e.ParamName), Value = e.ParamID.ToString() });
                }

                List<User> users = db.Users.OrderBy(u => u.FirstName).ToList();
                List<SelectListItem> userList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (User e in users)
                {
                    userList.Add(new SelectListItem { Text = String.Format("{0} {1}", e.FirstName, e.LastName), Value = e.UserID.ToString() });
                }

                List<SelectListItem> changeTypes = new List<SelectListItem> {
                    new SelectListItem { Selected = true, Text = "", Value = "" },
                    new SelectListItem { Text = "Insert", Value = "Insert" },
                    new SelectListItem { Text = "Update", Value = "Update" },
                    new SelectListItem { Text = "Delete", Value = "Delete" }
                };

                ViewBag.ParamList = paramList;
                ViewBag.ChangeTypes = changeTypes;
                ViewBag.Users = userList;
                ViewBag.SortDirection = SortDirection == "asc" ? "desc" : "asc";

                return PartialView("_GetParametersLog", logs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetParametersLog", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetUsersLog(string sortOrder, string SortDirection, int? page, String Username, String FirstName, String LastName, int? AuditUser, String AuditDateFrom, String AuditDateTo, String ChangeType)
        {
            try
            {
                sortOrder = String.IsNullOrEmpty(sortOrder) ? "FirstName" : sortOrder;
                SortDirection = String.IsNullOrEmpty(SortDirection) ? "desc" : SortDirection;
                ViewBag.CurrentSort = sortOrder;

                int pageSize = 25;
                int pageNumber = (page ?? 1);

                DateTime? auditDateFrom = String.IsNullOrEmpty(AuditDateFrom) ? new DateTime() : Convert.ToDateTime(AuditDateFrom);
                DateTime? auditDateTo = String.IsNullOrEmpty(AuditDateTo) ? new DateTime() : Convert.ToDateTime(AuditDateTo);

                List<User_Log> logs = db.User_Log
                    .Where(e => (String.IsNullOrEmpty(Username) || e.Username.StartsWith(Username)) &&
                        (String.IsNullOrEmpty(FirstName) || e.FirstName.StartsWith(FirstName)) &&
                        (String.IsNullOrEmpty(LastName) || e.LastName.StartsWith(LastName)) &&
                        (AuditUser == null || e.ChangedBy == AuditUser) &&
                        (auditDateFrom.Value.Year == 0001 || e.ChangeDate > auditDateFrom) &&
                        (auditDateTo.Value.Year == 0001 || e.ChangeDate < auditDateTo) &&
                        (String.IsNullOrEmpty(ChangeType) || e.ChangeType.Contains(ChangeType))
                        ).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

                ViewBag.Username = Username;
                ViewBag.FirstName = FirstName;
                ViewBag.LastName = LastName;
                ViewBag.AuditUser = AuditUser;
                ViewBag.AuditDateFrom = AuditDateFrom;
                ViewBag.AuditDateTo = AuditDateTo;
                ViewBag.ChangeType = ChangeType;

                switch (sortOrder)
                {
                    case "Username":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.Username).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.Username).ThenBy(m => m.LogID).ToList();
                        break;

                    case "FirstName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.FirstName).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.FirstName).ThenBy(m => m.LogID).ToList();
                        break;

                    case "LastName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.LastName).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.LastName).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditType":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditDate":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditBy":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        break;
                }

                List<User> users = db.Users.OrderBy(u => u.FirstName).ToList();
                List<SelectListItem> userList = new List<SelectListItem>();
                SelectListItem iempty = new SelectListItem { Text = "", Value = "", Selected = true };
                userList.Add(iempty);

                foreach (User e in users)
                {
                    SelectListItem i = new SelectListItem();
                    i.Text = String.Format("{0} {1}", e.FirstName, e.LastName);
                    i.Value = e.UserID.ToString();
                    userList.Add(i);
                }

                List<SelectListItem> changeTypes = new List<SelectListItem> {
                    new SelectListItem { Selected = true, Text = "", Value = "" },
                    new SelectListItem { Text = "Insert", Value = "Insert" },
                    new SelectListItem { Text = "Update", Value = "Update" },
                    new SelectListItem { Text = "Delete", Value = "Delete" }
                };

                ViewBag.ChangeTypes = changeTypes;
                ViewBag.Users = userList;
                ViewBag.SortDirection = SortDirection == "asc" ? "desc" : "asc";

                return PartialView("_GetUsersLog", logs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetUsersLog", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetWorkflowLog(string sortOrder, string SortDirection, int? page, String ItemType, String ItemName, int? AuditWorkflow, String AuditDateFrom, String AuditDateTo, String ChangeType)
        {
            try
            {
                sortOrder = String.IsNullOrEmpty(sortOrder) ? "ChangeDate" : sortOrder;
                SortDirection = String.IsNullOrEmpty(SortDirection) ? "desc" : SortDirection;
                ViewBag.CurrentSort = sortOrder;

                int pageSize = 25;
                int pageNumber = (page ?? 1);

                DateTime? auditDateFrom = String.IsNullOrEmpty(AuditDateFrom) ? new DateTime() : Convert.ToDateTime(AuditDateFrom);
                DateTime? auditDateTo = String.IsNullOrEmpty(AuditDateTo) ? new DateTime() : Convert.ToDateTime(AuditDateTo);

                List<Workflow_Log> logs = db.Workflow_Log
                    .Where(e => (String.IsNullOrEmpty(ItemType) || e.ItemType == ItemType) &&
                        (String.IsNullOrEmpty(ItemName) || e.ItemName.StartsWith(ItemName)) &&
                        (AuditWorkflow == null || e.ChangedBy == AuditWorkflow) &&
                        (auditDateFrom.Value.Year == 0001 || e.ChangeDate > auditDateFrom) &&
                        (auditDateTo.Value.Year == 0001 || e.ChangeDate < auditDateTo) &&
                        (String.IsNullOrEmpty(ChangeType) || e.ChangeType.Contains(ChangeType))
                        ).OrderBy(e => e.LogID).ToList();

                ViewBag.ItemType = ItemType;
                ViewBag.ItemName = ItemName;
                ViewBag.AuditWorkflow = AuditWorkflow;
                ViewBag.AuditDateFrom = AuditDateFrom;
                ViewBag.AuditDateTo = AuditDateTo;
                ViewBag.ChangeType = ChangeType;

                switch (sortOrder)
                {
                    case "ChangeDate":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        break;

                    case "ItemType":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ItemType).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ItemType).ThenBy(m => m.LogID).ToList();
                        break;

                    case "ItemName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ItemName).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ItemName).ThenBy(m => m.LogID).ToList();
                        break;
                    case "AuditType":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeType).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditDate":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangeDate).ThenBy(m => m.LogID).ToList();
                        break;

                    case "AuditBy":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        else
                            logs = logs.OrderBy(m => m.ChangedBy).ThenBy(m => m.LogID).ToList();
                        break;
                }

                List<SelectListItem> itemTypes = new List<SelectListItem> {
                    new SelectListItem { Selected = true, Text = "", Value = "" },
                    new SelectListItem { Text = "Workflow", Value = "Workflow" },
                    new SelectListItem { Text = "Workflow Item", Value = "Workflow Item" },
                    new SelectListItem { Text = "Library Item", Value = "Library Item" }
                };

                List<SelectListItem> changeTypes = new List<SelectListItem> {
                    new SelectListItem { Selected = true, Text = "", Value = "" },
                    new SelectListItem { Text = "Insert", Value = "Insert" },
                    new SelectListItem { Text = "Update", Value = "Update" },
                    new SelectListItem { Text = "Delete", Value = "Delete" }
                };

                List<String> itemNames = (from w in db.Workflow_Log select w.ItemName).Distinct().ToList();
                List<SelectListItem> itemNameList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (String n in itemNames)
                {
                    itemNameList.Add(new SelectListItem { Text = n, Value = n });
                }

                List<User> users = db.Users.OrderBy(u => u.FirstName).ToList();
                List<SelectListItem> userList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (User e in users)
                {
                    userList.Add(new SelectListItem { Text = String.Format("{0} {1}", e.FirstName, e.LastName), Value = e.UserID.ToString() });
                }

                ViewBag.ItemNames = itemNameList;
                ViewBag.Users = userList;
                ViewBag.ChangeTypes = changeTypes;
                ViewBag.ItemTypes = itemTypes;
                ViewBag.SortDirection = SortDirection == "asc" ? "desc" : "asc";

                return PartialView("_GetWorkflowLog", logs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetWorkflowLog", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public ActionResult GetWorkflowRunLog(string sortOrder, string SortDirection, int? page, int? ItemID, int? ResultID, String FirstName, String LastName, int? WorkflowID, int? AuditUserID, String AuditDateFrom, String AuditDateTo, String ChangeType)
        {
            try
            {
                sortOrder = String.IsNullOrEmpty(sortOrder) ? "TimeCompleted" : sortOrder;
                SortDirection = String.IsNullOrEmpty(SortDirection) ? "desc" : SortDirection;
                ViewBag.CurrentSort = sortOrder;

                int pageSize = 25;
                int pageNumber = (page ?? 1);

                DateTime? auditDateFrom = String.IsNullOrEmpty(AuditDateFrom) ? new DateTime() : Convert.ToDateTime(AuditDateFrom);
                DateTime? auditDateTo = String.IsNullOrEmpty(AuditDateTo) ? new DateTime() : Convert.ToDateTime(AuditDateTo);

                List<vw_RunResults_Log> logs = db.vw_RunResults_Log
                    .Where(e => (ItemID == null || e.WFItemID == ItemID) &&
                        (ResultID == null || e.ResultID == ResultID) &&
                        (String.IsNullOrEmpty(FirstName) || e.FirstName.Contains(FirstName)) &&
                        (String.IsNullOrEmpty(LastName) || e.LastName.Contains(LastName)) &&
                        (AuditUserID == null || e.RunByUserID == AuditUserID) &&
                        (WorkflowID == null || e.WorkflowID == WorkflowID) &&
                        (auditDateFrom.Value.Year == 0001 || e.TimeCompleted > auditDateFrom) &&
                        (auditDateTo.Value.Year == 0001 || e.TimeCompleted < auditDateTo)
                        ).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

                ViewBag.ItemID = ItemID;
                ViewBag.ResultID = ResultID;
                ViewBag.FirstName = FirstName;
                ViewBag.LastName = LastName;
                ViewBag.WorkflowID = WorkflowID;
                ViewBag.AuditDateFrom = AuditDateFrom;
                ViewBag.AuditDateTo = AuditDateTo;
                ViewBag.ChangeType = ChangeType;

                switch (sortOrder)
                {
                    case "ItemName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ItemName).ToList();
                        else
                            logs = logs.OrderBy(m => m.ItemName).ToList();
                        break;

                    case "FirstName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.FirstName).ToList();
                        else
                            logs = logs.OrderBy(m => m.FirstName).ToList();
                        break;

                    case "LastName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.LastName).ToList();
                        else
                            logs = logs.OrderBy(m => m.LastName).ToList();
                        break;

                    case "ResultStatus":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.ResultStatus).ToList();
                        else
                            logs = logs.OrderBy(m => m.ResultStatus).ToList();
                        break;

                    case "WorkflowName":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.WorkflowName).ToList();
                        else
                            logs = logs.OrderBy(m => m.WorkflowName).ToList();
                        break;

                    case "RunByUser":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.RunByUser).ToList();
                        else
                            logs = logs.OrderBy(m => m.RunByUser).ToList();
                        break;

                    case "Started":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.TimeStarted).ToList();
                        else
                            logs = logs.OrderBy(m => m.TimeStarted).ToList();
                        break;

                    case "Completed":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.TimeCompleted).ToList();
                        else
                            logs = logs.OrderBy(m => m.TimeCompleted).ToList();
                        break;

                    case "RunDate":
                        if (SortDirection == "desc")
                            logs = logs.OrderByDescending(m => m.RunDateFullDate).ToList();
                        else
                            logs = logs.OrderBy(m => m.RunDateFullDate).ToList();
                        break;
                }

                List<RunResultStatu> RunStatus = db.RunResultStatus.OrderBy(u => u.ResultStatus).ToList();
                List<SelectListItem> RunStatusList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (RunResultStatu e in RunStatus)
                {
                    RunStatusList.Add(new SelectListItem { Text = e.ResultStatus, Value = e.ResultID.ToString() });
                }

                List<LibraryItem> Items = db.LibraryItems.OrderBy(u => u.ItemName).ToList();
                List<SelectListItem> ItemList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (LibraryItem e in Items)
                {
                    ItemList.Add(new SelectListItem { Text = e.ItemName, Value = e.ItemID.ToString() });
                }

                List<Workflow> Workflows = db.Workflows.OrderBy(u => u.WorkflowName).ToList();
                List<SelectListItem> WorkflowList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (Workflow e in Workflows)
                {
                    WorkflowList.Add(new SelectListItem { Text = e.WorkflowName, Value = e.WorkflowID.ToString() });
                }

                List<User> users = db.Users.OrderBy(u => u.FirstName).ToList();
                List<SelectListItem> userList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "", Selected = true } };
                foreach (User e in users)
                {
                    userList.Add(new SelectListItem { Text = String.Format("{0} {1}", e.FirstName, e.LastName), Value = e.UserID.ToString() });
                }

                List<SelectListItem> changeTypes = new List<SelectListItem> {
                    new SelectListItem { Selected = true, Text = "", Value = "" },
                    new SelectListItem { Text = "Insert", Value = "Insert" },
                    new SelectListItem { Text = "Update", Value = "Update" },
                    new SelectListItem { Text = "Delete", Value = "Delete" }
                };

                ViewBag.WorkflowList = WorkflowList;
                ViewBag.ItemList = ItemList;
                ViewBag.ChangeTypes = changeTypes;
                ViewBag.Users = userList;
                ViewBag.RunStatusList = RunStatusList;
                ViewBag.SortDirection = SortDirection == "asc" ? "desc" : "asc";

                return PartialView("_GetWorkflowRunLog", logs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/GetWorkflowRunLog", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }
    }
}
