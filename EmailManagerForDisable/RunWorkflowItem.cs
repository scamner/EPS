using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Reflection;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public ItemRunResult RunItem(int EmpID, String RunPayload)
        {
            List<RunPayloadModel> pl = new List<RunPayloadModel>();
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();

            String jsonPL = String.IsNullOrEmpty(RunPayload) ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(pl);

            try
            {
                if (!String.IsNullOrEmpty(RunPayload))
                {
                    pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);
                    pl.Add(new RunPayloadModel());
                }

                Utilities util = new Utilities();

                Employee manager = db.Employees.Where(e => e.EmpID == emp.ReportsTo).FirstOrDefault();

                if (manager == null)
                {
                    throw new Exception(String.Format("There is no manager assigned to {0} {1}.", emp.FirstName, emp.LastName));
                }

                if (String.IsNullOrEmpty(manager.Email))
                {
                    throw new Exception(String.Format("The manager assigned to {0} {1} does not have an email set.", emp.FirstName, emp.LastName));
                }

                String emailServer = util.GetParam("EmailServer", "smtp (email) server name");
                String from = util.GetParam("EmailFrom", "email address to send from");
                String to = manager.Email;
                String body = util.GetParam("DisabledNotifyBody", "message to managers to notify them of a disabled employee");
                String subject = util.GetParam("DisabledNotifySubject", "subject line for the email to send managers to notify them of a disabled employee");

                body = body.Replace("[FirstName]", emp.FirstName);
                body = body.Replace("[LastName]", emp.LastName);

                foreach (PropertyInfo prop in typeof(Employee).GetProperties())
                {
                    body = body.Replace(String.Format("[{0}]", prop.Name), prop.GetValue(emp).ToString());
                    subject = subject.Replace(String.Format("[{0}]", prop.Name), prop.GetValue(emp).ToString());
                }

                util.SendEmail(from, to, null, null, subject, body);

                return new ItemRunResult { ResultID = 2, ResultText = String.Format("The email was sent to {0} {1}.", manager.FirstName, manager.LastName), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
            catch (Exception ex)
            {
                return new ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
        }
    }
}
