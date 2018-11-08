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

                if (!System.IO.Directory.Exists(userFolder))
                {
                    System.IO.Directory.CreateDirectory(userFolder);
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

                foreach (string dirPath in Directory.GetDirectories(homeFolder, "*", SearchOption.AllDirectories))
                {
                    string newPath = dirPath.Replace(homeFolder, userFolder);
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                }

                foreach (string oldFile in Directory.GetFiles(homeFolder, "*.*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(oldFile, System.IO.FileAttributes.Normal);
                }

                foreach (string oldFile in Directory.GetFiles(homeFolder, "*.*", SearchOption.AllDirectories))
                {
                    String newFile = oldFile.Replace(homeFolder, userFolder);

                    if (!File.Exists(newFile))
                    {
                        File.SetAttributes(oldFile, System.IO.FileAttributes.Normal);
                        File.Move(oldFile, newFile);
                    }
                }

                System.IO.Directory.Delete(homeFolder, true);

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1}'s home folder documents were moved from '{3}' to '{2}'.", emp.FirstName, emp.LastName, userFolder, homeFolder), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}