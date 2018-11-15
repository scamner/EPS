using DataLayer;
using System;
using System.Linq;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public Utilities.ItemRunResult RunItem(int EmpID, RunPayload RunPayload)
        {
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();

            try
            {
                Utilities util = new Utilities();

                String emailServer = util.GetParam("SMTPServer", "smtp (email) server name");
                String from = util.GetParam("EmailFrom", "email address to send from");
                String to = util.GetParam("HelpDeskEmail", "email address to send to for helpdesk tickets");
                String body = util.GetParam("ClonedLaptopBody", "message to create a helpdesk ticket for cloned laptops");
                String subject = util.GetParam("ClonedLaptopSubject", "subject line to create a helpdesk ticket for cloned laptops");

                util.SendEmail(EmpID, from, to, null, null, subject, body);

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("The email was sent to '{0}' regarding {1} {2} for their cloned laptop.", to, emp.FirstName, emp.LastName), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}
