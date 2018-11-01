using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Web.Script.Serialization;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            String AD_Admin = GetParam("ADUsername", "Active Directory Administrator username");
            String AD_Password = GetParam("ADPassword", "Active Directory Administrator password");
            String ExchangeServer = GetParam("ExchangeServer", "Exchange Server Powershell URL");

            var secure = new SecureString();

            foreach (char c in AD_Password.ToCharArray())
            {
                secure.AppendChar(c);
            }

            PSCredential credential = new PSCredential(AD_Admin, secure);

            int iRemotePort = 5985;
            string strShellURI = @"http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            string strAppName = @"/wsman";

            AuthenticationMechanism auth = AuthenticationMechanism.Negotiate;

            WSManConnectionInfo ci = new WSManConnectionInfo(
                false,
                ExchangeServer,
                iRemotePort,
                strAppName,
                strShellURI,
                credential);

            ci.AuthenticationMechanism = auth;

            Runspace runspace = RunspaceFactory.CreateRunspace(ci);
            runspace.Open();

            PowerShell psh = PowerShell.Create();
            psh.AddCommand("Out-String");
            psh.Commands.AddScript(scriptText);
            psh.Runspace = runspace;
            Collection<PSObject> results = psh.Invoke();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            return stringBuilder.ToString();
        }

        public RunPayload SetPayload(int RunID, int ItemID, String HtmlAnswers)
        {
            RunPayload pl = db.RunPayloads.Where(r => r.RunID == RunID).FirstOrDefault();

            if (pl == null)
            {
                RunPayload newRPL = new RunPayload { RunID = RunID };
                db.RunPayloads.Add(newRPL);
                db.SaveChanges();

                pl = db.RunPayloads.Where(r => r.RunID == RunID).FirstOrDefault();
            }

            int colonIndex = HtmlAnswers.IndexOf(":");
            String itemID = HtmlAnswers.Substring(0, colonIndex);
            String elemHtml = HtmlAnswers.Substring(colonIndex + 1, HtmlAnswers.Length - (colonIndex + 1));

            String[] elems = elemHtml.Split('&');

            foreach (string e in elems)
            {
                int eqIndex = e.IndexOf("=");
                String elemID = e.Substring(0, eqIndex);
                String elemValue = e.Substring(eqIndex + 1, e.Length - (eqIndex + 1));

                RunPayloadItem pli = new RunPayloadItem { ElementID = elemID, ElementValue = elemValue, ItemID = ItemID, PayloadID = pl.PayloadID };
                db.RunPayloadItems.Add(pli);
                db.SaveChanges();
            }

            pl = db.RunPayloads.Where(r => r.RunID == RunID).FirstOrDefault();

            return pl;
        }

        public class ItemRunResult
        {
            public int ResultID { get; set; }
            public String ResultText { get; set; }
            public DateTime TimeDone { get; set; }
        }
    }
}
