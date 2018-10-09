using DataLayer;
using EPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using Newtonsoft.Json;

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
                List<vwRunWorkflow> runs = db.vwRunWorkflows.
                    Where(r => (r.RunStatus == "Pending" || r.StartTime >= DateTime.Now.AddSeconds(-30))).OrderBy(e => e.StartTime).ToList();

                List<int> RunIDs = runs.Select(r => r.RunID).Distinct().ToList();
                List<RunResult> Results = db.RunResults.Where(r => RunIDs.Contains(r.RunID)).ToList();

                List<GetWorkflowModel> model = new List<GetWorkflowModel>();

                foreach (vwRunWorkflow wf in runs)
                {
                    GetWorkflowModel m = new GetWorkflowModel
                    {
                        RunID = wf.RunID,
                        EmpID = wf.EmpID,
                        FirstName = wf.FirstName,
                        ItemCount = wf.ItemCount,
                        LastName = wf.LastName,
                        ResultItems = Results.Where(r => r.RunID == wf.RunID).OrderBy(r => r.TimeCompleted).ToList(),
                        RunStatus = wf.RunStatus,
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

                return Json(new { Error = error, JsonRequestBehavior.AllowGet });
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

                return Json(new { Error = error, JsonRequestBehavior.AllowGet });
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

                return Json(new { Error = error, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpGet]
        public JsonResult LoadRunWorkflows()
        {
            try
            {
                List<Workflow> wf = db.Workflows.Where(w => w.Disabled == false).OrderBy(w => w.WorkflowName).ToList();

                var list = JsonConvert.SerializeObject(wf, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

                foreach (Workflow wf1 in wf)
                {
                    wf1.WorkflowItems = wf1.WorkflowItems.OrderBy(w => w.RunOrder).ToList();
                }

                return Json(new { Error = "", WorkFlows = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("Dashboard/LoadRunWorkflows", ex, "");

                return Json(new { Error = error, JsonRequestBehavior.AllowGet });
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

                    return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    String error = util.ParseError(ex);
                    util.WriteErrorToLog("Dashboard/RunWorkflow", ex, String.Format("Emp ID: {0}", EmpID));

                    return Json(new { Error = error, JsonRequestBehavior.AllowGet });
                }
            }
        }
    }
}