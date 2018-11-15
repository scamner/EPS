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

                String disableGroup = util.GetParam("DisableDistroGroupEmail", "Disable Distro Group email address");
                String emailServer = util.GetParam("SMTPServer", "smtp (email) server name");
                String from = util.GetParam("EmailFrom", "email address to send from");
                String to = disableGroup;
                String body = util.GetParam("DisableDistroGroupBody", "message to disable distro group to notify them of a disabled employee");
                String subject = util.GetParam("DisableDistroGroupSubject", "subject line for the email to send the disable distro group to notify them of a disabled employee");

                util.SendEmail(EmpID, from, to, null, null, subject, body);

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("The email was sent to '{0}' regarding {1} {2}.", disableGroup, emp.FirstName, emp.LastName), TimeDone = DateTime.Now};
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now};
            }
        }
    }
}
