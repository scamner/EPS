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

                String disableGroup = util.GetParam("DisableDistroGroupEmail", "Disable Distro Group email address");
                String emailServer = util.GetParam("EmailServer", "smtp (email) server name");
                String from = util.GetParam("EmailFrom", "email address to send from");
                String to = disableGroup;
                String body = util.GetParam("DisableDistroGroupBody", "message to disable distro group to notify them of a disabled employee");
                String subject = util.GetParam("DisableDistroGroupSubject", "subject line for the email to send the disable distro group to notify them of a disabled employee");

                foreach (PropertyInfo prop in typeof(Employee).GetProperties())
                {
                    body = body.Replace(String.Format("[{0}]", prop.Name), prop.GetValue(emp).ToString());
                    subject = subject.Replace(String.Format("[{0}]", prop.Name), prop.GetValue(emp).ToString());
                }

                util.SendEmail(from, to, null, null, subject, body);

                return new ItemRunResult { ResultID = 2, ResultText = String.Format("The email was sent to '{0}' regarding {1} {2}.", disableGroup, emp.FirstName, emp.LastName), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
            catch (Exception ex)
            {
                return new ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
        }
    }

    public class ItemRunResult
    {
        public int ResultID { get; set; }
        public String ResultText { get; set; }
        public DateTime TimeDone { get; set; }
        public String RunPayload { get; set; }
    }

    public class RunPayloadModel
    {
        public String TargetLibraryName { get; set; }
        public String PayloadParent { get; set; }
        public String TargetPayload { get; set; }
    }
}
