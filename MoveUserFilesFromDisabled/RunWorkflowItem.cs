using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
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
                DirectoryEntry DE = (DirectoryEntry)user.GetUnderlyingObject();
                String userFolder = String.Format("{0}\\{1}", disabledFolderPath, user.SamAccountName);

                if (!System.IO.Directory.Exists(disabledFolderPath))
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("The disabled users folder '{0}' could not be found.", disabledFolderPath), TimeDone = DateTime.Now };
                }

                if (user == null)
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now };
                }

                var HD = DE.InvokeGet("TerminalServicesHomeDirectory");

                if (HD == null)
                {
                    HD = user.HomeDirectory;
                }

                String homeFolder = HD == null ? "" : HD.ToString();

                if (String.IsNullOrEmpty(homeFolder))
                {
                    return new Utilities.ItemRunResult { ResultID = 5, ResultText = String.Format("{0} {1}'s home folder is not set.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now };
                }

                if (!System.IO.Directory.Exists(homeFolder))
                {
                    System.IO.Directory.CreateDirectory(homeFolder);
                }

                DirectoryInfo dirInfo = new DirectoryInfo(homeFolder);
                List<String> allFiles = Directory.GetFiles(userFolder, "*.*", SearchOption.AllDirectories).ToList();

                foreach (string file in allFiles)
                {
                    FileInfo mFile = new FileInfo(file);

                    if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                    {
                        mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                    }
                }

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1}'s home folder documents were moved from '{3}' to '{2}'.", emp.FirstName, emp.LastName, homeFolder, userFolder), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}