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

namespace EPS.Controllers
{
    public class DashboardController : Controller
    {
        DataLayer.EPSEntities db = new DataLayer.EPSEntities();
        Utilities util = new Utilities();

        public ActionResult Index()
        {
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

        public ActionResult GetRuns()
        {
            try
            {
                DateTime checkTime = DateTime.Now.AddMinutes(-5);

                List<vwRunWorkflow> runs = db.vwRunWorkflows.
                    Where(r => (r.RunStatus == "Pending" || r.StartTime >= checkTime)).OrderBy(e => e.StartTime).ToList();

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
                        WorkflowID = wf.WorkflowID
                    };

                    model.Add(m);
                }

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

                return Json(new { Error = "", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/AddEmployee", ex, String.Format("First Name: {0}, Last Name: {1}", FirstName, LastName));

                return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SetEmployeeAsManager(int EmpID, Boolean IsManager)
        {
            try
            {
                int isManagerCount = db.Employees.Where(e => e.ReportsTo == EmpID).Count();

                if (isManagerCount > 0)
                {
                    return Json(new { Error = "That employee is assigned as a manager to other employees. Please change that first.", JsonRequestBehavior.AllowGet });
                }

                Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
                emp.IsManager = IsManager;
                db.SaveChanges();

                Employees_Log log = new Employees_Log
                {
                    IsManager = Convert.ToBoolean(IsManager),
                    EmpID = emp.EmpID,
                    ChangeDate = DateTime.Now,
                    ChangedBy = util.GetLoggedOnUser().UserID,
                    ChangeType = "Update"
                };

                db.Employees_Log.Add(log);
                db.SaveChanges();

                return Json(new { Error = "", ManagerName = String.Format("{0} {1}", emp.FirstName, emp.LastName), JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/SetEmployeeAsManager", ex, String.Format("Emp ID: {0}, Is Manager: {1}", EmpID.ToString(), IsManager.ToString()));

                return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SetEmployeeReportTo(int EmpID, String ManagerID)
        {
            try
            {
                Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();

                if (Convert.ToInt32(ManagerID) == 0) { emp.ReportsTo = null; }
                else { emp.ReportsTo = Convert.ToInt32(ManagerID); }
                db.SaveChanges();

                Employees_Log log = new Employees_Log
                {
                    ReportsTo = Convert.ToInt32(ManagerID),
                    EmpID = emp.EmpID,
                    ChangeDate = DateTime.Now,
                    ChangedBy = util.GetLoggedOnUser().UserID,
                    ChangeType = "Update"
                };

                db.Employees_Log.Add(log);
                db.SaveChanges();

                return Json(new { Error = "", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/SetEmployeeReportTo", ex, String.Format("Emp ID: {0}, Manager ID: {1}", EmpID.ToString(), ManagerID.ToString()));

                return Json(new { Error = error }, JsonRequestBehavior.AllowGet);
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
        public JsonResult RunWorkflow(int EmpID, int WorkflowID, int[] ItemIDs, String[] HtmlOptions)
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

                    db.RunWorkflows.Add(run);
                    db.SaveChanges();

                    foreach (int itemID in ItemIDs)
                    {
                        String htmlOption = HtmlOptions.Where(h => h.ToString()
                        .StartsWith(String.Format("{0}:", itemID))).FirstOrDefault().ToString();

                        RunItem ri = new RunItem
                        {
                            HtmlAnswers = htmlOption,
                            RunID = run.RunID,
                            WFItemID = itemID
                        };

                        db.RunItems.Add(ri);
                        db.SaveChanges();
                    }

                    tran.Commit();

                    RunWorkflows.ExecuteWorkflows exec = new RunWorkflows.ExecuteWorkflows();
                    Task.Run(() => exec.RunWorkflow(run.RunID));

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

        public ActionResult AddLibraryItem(NewLibraryItemModel model)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    model.LibraryPathFile.InputStream.CopyTo(memoryStream);
                }

                return null;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
