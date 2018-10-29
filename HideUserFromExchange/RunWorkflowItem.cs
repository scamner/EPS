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
        public ItemRunResult RunItem(int EmpID, String RunPayload)
        {
            List<RunPayloadModel> pl = new List<RunPayloadModel>();
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Utilities util = new Utilities();

            Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
            String domain = util.GetParam("ADDomain", "Active Directory Domain name");
            String emailServer = util.GetParam("ExchangePS_URL", "email server powershell URL");

            String jsonPL = String.IsNullOrEmpty(RunPayload) ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(pl);

            try
            {
                if (!String.IsNullOrEmpty(RunPayload))
                {
                    pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);
                    pl.Add(new RunPayloadModel());
                }

                StringBuilder sbScript = new StringBuilder();

                //Must run Add-WindowsFeature RSAT-AD-PowerShell to make sure AD Powershell is active

                sbScript.Append("$username = \"Shawntest\\Administrator\"" + Environment.NewLine);
                sbScript.Append("$password = \"Nin^2020\"" + Environment.NewLine);
                sbScript.Append("$secstr = New-Object -TypeName System.Security.SecureString" + Environment.NewLine);
                sbScript.Append("$password.ToCharArray() | ForEach-Object {$secstr.AppendChar($_)}" + Environment.NewLine);
                sbScript.Append("$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $username, $secstr" + Environment.NewLine);

                sbScript.Append("$Session = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri https://testserver.shawntest.local/powershell/ -Credential $cred -Authentication Basic -AllowRedirection" + Environment.NewLine);
               
                sbScript.Append("Set-ADUser silcamner -Add @{msExchHideFromAddressLists=\"TRUE\"}");

                String finalScript = sbScript.ToString();

                String result = util.RunPSScript(finalScript);

                return new ItemRunResult { ResultID = 2, ResultText = String.Format("The user was hidden from the Exchange address lists."), TimeDone = DateTime.Now, RunPayload = jsonPL };
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