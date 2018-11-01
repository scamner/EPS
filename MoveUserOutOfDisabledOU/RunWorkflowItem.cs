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
        public Utilities.ItemRunResult RunItem(int EmpID, RunPayload RunPayload)
        {
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Utilities util = new Utilities();

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
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now };
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
                    return new Utilities.ItemRunResult { ResultID = 5, ResultText = String.Format("{0} is already in the default Users container.", emp.Username), TimeDone = DateTime.Now };
                }

                userOU.MoveTo(defaultCN);
                userOU.Close();

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1} was moved to the default Users container in Active Directory.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now};
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now};
            }
        }
    }
}