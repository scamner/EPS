using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
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
            String domain = util.GetParam("ADDomain", "Active Directory Domain name");

            String Script = "";

            try
            {
                Script = "Set-ADUser " + emp.Username + " -Replace @{msExchHideFromAddressLists=\"TRUE\"}";

                String result = util.RunPSScript(Script);

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1} was hidden from the Exchange address lists.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now};
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}... Script Run: {1}", ex.Message, Script), TimeDone = DateTime.Now};
            }
        }
    }
}