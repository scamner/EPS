using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

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
                String disabledFolderPath = util.GetParam("DisabledFolderPath", "disabled users folder path");

                if (disabledFolderPath.EndsWith("\\"))
                {
                    disabledFolderPath = disabledFolderPath.Remove(disabledFolderPath.Length - 1, 1);
                }

                PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, adminName, password);
                UserPrincipal user = UserPrincipal.FindByIdentity(context, emp.Username);
                String userFolder = String.Format("{0}\\{1}", disabledFolderPath, user.SamAccountName);
                String groupNameFilePath = String.Format("{0}\\ADGroups.txt", userFolder);
                StringBuilder sbGroupNamesFile = new StringBuilder();

                if (!System.IO.Directory.Exists(disabledFolderPath))
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("The disabled users folder '{0}' could not be found.", disabledFolderPath), TimeDone = DateTime.Now };
                }

                if (user == null)
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now };
                }

                if (!System.IO.Directory.Exists(userFolder))
                {
                    System.IO.Directory.CreateDirectory(userFolder);
                }

                PrincipalSearchResult<Principal> groups = user.GetGroups();

                foreach (var g in groups)
                {
                    sbGroupNamesFile.AppendFormat("{0}" + Environment.NewLine, g.Name);
                }

                if (System.IO.File.Exists(groupNameFilePath))
                {
                    System.IO.File.Delete(groupNameFilePath);
                }

                System.IO.File.WriteAllText(groupNameFilePath, sbGroupNamesFile.ToString());

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1}'s Active Directory groups have been documented in '{2}'.", emp.FirstName, emp.LastName, groupNameFilePath), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}