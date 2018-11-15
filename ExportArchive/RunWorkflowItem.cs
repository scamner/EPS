using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.DirectoryServices;
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
            String userFolder = String.Format("{0}\\{1}\\{2}\\MailBox_Archive_Export", disabledFolderPath, DateTime.Now.Year.ToString(), user.SamAccountName);

            if (String.IsNullOrEmpty(user.EmailAddress))
            {
                return new Utilities.ItemRunResult { ResultID = 5, ResultText = String.Format("{0} {1} does not have an email address in Active Directory.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now };
            }

            if (!System.IO.Directory.Exists(disabledFolderPath))
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("The disabled users folder '{0}' could not be found.", disabledFolderPath), TimeDone = DateTime.Now };
            }

            if (user == null)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now };
            }

            var archiveName = DE.InvokeGet("msExchArchiveName");

            if (archiveName == null || String.IsNullOrEmpty(archiveName.ToString()))
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} does not have an archive folder set.", emp.Username), TimeDone = DateTime.Now };
            }

            if (!System.IO.Directory.Exists(userFolder))
            {
                System.IO.Directory.CreateDirectory(userFolder);
            }

            String Script = "";

            try
            {
                Script = "$CallEMS = \". '$env:ExchangeInstallPath\\bin\\RemoteExchange.ps1'; Connect-ExchangeServer -auto -ClientApplication:ManagementShell\"; Invoke-Expression $CallEMS; ";
                Script = Script + String.Format("New-MailboxExportRequest -Mailbox {0} -FilePath \"{1}\\MailboxExport.pst\" -IsArchive -Priority High", emp.Username, userFolder);

                String result = util.RunPSScript(Script);

                if (!String.IsNullOrEmpty(result))
                {
                    throw new Exception(result);
                }

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1}'s archive export was started going to {2}. Exchange Server will continue to process until it is complete.", emp.FirstName, emp.LastName, userFolder), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}... Script Run: {1}", ex.Message, Script), TimeDone = DateTime.Now };
            }
        }
    }
}