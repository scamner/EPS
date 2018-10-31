using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

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
                Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
                String domain = util.GetParam("ADDomain", "Active Directory domain");
                String adminName = util.GetParam("ADUsername", "Active Directory admin user");
                String password = util.GetParam("ADPassword", "Active Directory admin user password");
                String ou = util.GetParam("DisabledUsersOU", "Active Directory Disabled Users OU path");
                String defCN = util.GetParam("DefaultUsersCN", "Active Directory default Users container path");

                PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, adminName, password);
                UserPrincipal user = UserPrincipal.FindByIdentity(context, emp.Username);

                if (user == null)
                {
                    return new ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now, RunPayload = jsonPL };
                }

                Boolean isInOUAlready = false;

                DirectoryEntry userOU = new DirectoryEntry("LDAP://" + user.DistinguishedName, adminName, password);
                DirectoryEntry defaultCN = new DirectoryEntry("LDAP://" + defCN, adminName, password);

                String OUCheckUser = userOU.Path.ToString();

                if (OUCheckUser.EndsWith(defCN))
                {
                    isInOUAlready = true;
                }

                if (isInOUAlready == true)
                {
                    return new ItemRunResult { ResultID = 5, ResultText = String.Format("{0} is already in the default Users container.", emp.Username), TimeDone = DateTime.Now, RunPayload = jsonPL };
                }

                userOU.MoveTo(defaultCN);
                userOU.Close();

                if (!String.IsNullOrEmpty(RunPayload))
                {
                    pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);
                    pl.Add(new RunPayloadModel());
                }

                return new ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1} was moved to the default Users container in Active Directory.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
            catch (Exception ex)
            {
                return new ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
        }
    }

    public class ItemRunResult
    {
        public int ResultID { get; set; }
        public String ResultText { get; set; }
        public DateTime TimeDone { get; set; }
        public String RunPayload { get; set; }
    }

    public class RunPayloadModel
    {
        public String TargetLibraryName { get; set; }
        public String PayloadParent { get; set; }
        public String TargetPayload { get; set; }
    }
}