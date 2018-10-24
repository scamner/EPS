using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DataLayer
{
    public class Utilities
    {
        EPSEntities db = new EPSEntities();

        public void SendEmail(String FromEmail, String ToEmail, String[] CC, String[] BCC, String Subject, String Body)
        {
            Parameter param = db.Parameters.Where(p => p.ParamName == "EmailServer").FirstOrDefault();

            if (param == null)
            {
                throw new Exception("The email server has not been setup in the parameters table. Please add it with a value name of 'EmailServer'.");
            }

            MailMessage message = new MailMessage(FromEmail, ToEmail);

            if (CC != null && CC.Length > 0)
            {
                foreach (String e in CC) { message.CC.Add(e); }
            }

            if (BCC != null && BCC.Length > 0)
            {
                foreach (String e in BCC) { message.Bcc.Add(e); }
            }

            message.Subject = Subject;
            message.IsBodyHtml = true;
            message.Body = Body;

            var client = new SmtpClient();
            client.Host = param.ParamValue;
            client.Port = 25;
            client.Send(message);
        }

        public String GetParam(String ParamName, String ErrorMessage)
        {
            Parameter param = db.Parameters.Where(p => p.ParamName == ParamName).FirstOrDefault();

            if (param == null)
            {
                throw new Exception(String.Format("The {0} is not configured in the parameters table. Please add a parameter with the name of '{1}' and the appropriate value.", ErrorMessage, ParamName));
            }

            return param.ParamValue;
        }

        public String RunPSScript(string scriptText)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(scriptText);

            pipeline.Commands.Add("Out-String");

            Collection<PSObject> results = pipeline.Invoke();

            runspace.Close();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
