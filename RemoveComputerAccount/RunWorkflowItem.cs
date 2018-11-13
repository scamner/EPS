using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
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
                String myName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                LibraryItem li = db.LibraryItems.Where(l => l.LibraryPath.EndsWith(myName + ".dll")).FirstOrDefault();

                String htmlOptions = li.HtmlOptions;

                String domain = util.GetParam("ADDomain", "Active Directory domain");
                String adminName = util.GetParam("ADUsername", "Active Directory admin user");
                String password = util.GetParam("ADPassword", "Active Directory admin user password");

                List<RunPayloadItem> thisPL = RunPayload.RunPayloadItems.Where(p => p.ItemID == li.ItemID).ToList();

                string compAcct = thisPL.Where(p => p.ElementID == "CompAcct").FirstOrDefault().ElementValue;

                PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, adminName, password);
                ComputerPrincipal comp = ComputerPrincipal.FindByIdentity
                        (context, compAcct);

                if (comp == null)
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", compAcct), TimeDone = DateTime.Now };
                }

                comp.Delete();

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("Computer name: {0} was removed from Active Directory.",compAcct), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}