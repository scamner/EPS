using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.DirectoryServices.AccountManagement;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public ItemRunResult RunItem(int EmpID, String RunPayload)
        {
            List<RunPayloadModel> pl = new List<RunPayloadModel>();
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Utilities util = new Utilities();

            String jsonPL = String.IsNullOrEmpty(RunPayload) ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(pl);

            try
            {
                if (!String.IsNullOrEmpty(RunPayload))
                {
                    pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);
                    pl.Add(new RunPayloadModel());
                }

                Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
                String domain = util.GetParam("ADDomain", "Active Directory domain");
                String adminName = util.GetParam("ADUsername", "Active Directory admin user");
                String password = util.GetParam("ADPassword", "Active Directory admin user password");

                PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, adminName, password);
                UserPrincipal user = UserPrincipal.FindByIdentity
                        (context, emp.Username);

                if (user == null)
                {
                    return new ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now, RunPayload = jsonPL };
                }

                if (user.Enabled == true)
                {
                    return new ItemRunResult { ResultID = 5, ResultText = String.Format("{0} was already enabled in Active Directory.", emp.Username), TimeDone = DateTime.Now, RunPayload = jsonPL };
                }

                user.Enabled = true;
                user.Save();

                return new ItemRunResult { ResultID = 2, ResultText = String.Format("Employee: {0} {1} was enabled in Active Directory.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
            catch (Exception ex)
            {
                return new ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
        }
    }
}


