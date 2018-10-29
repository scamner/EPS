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
            String ExchangePS_URL = GetParam("ExchangePS_URL", "Exchange Server Powershell URL");

            var secure = new SecureString();

            foreach (char c in AD_Password.ToCharArray())
            {
                secure.AppendChar(c);
            }

            PSCredential credential = new PSCredential(AD_Admin, secure);

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri(ExchangePS_URL), "http://schemas.microsoft.com/powershell/Microsoft.Exchange", credential);
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;
            connectionInfo.SkipCACheck = true;
            connectionInfo.SkipCNCheck = true;

            Runspace runspace = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace(connectionInfo);
            PowerShell powershell = PowerShell.Create();
            PSCommand command = new PSCommand();
            command.AddCommand("Out-String");
            powershell.Commands = command;
            powershell.Commands.AddScript(scriptText);

            runspace.Open();
            powershell.Runspace = runspace;
            Collection<PSObject> results = powershell.Invoke();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
