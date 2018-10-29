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

            String jsonPL = String.IsNullOrEmpty(RunPayload) ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(pl);

            String finalScript = "";

            try
            {
                if (!String.IsNullOrEmpty(RunPayload))
                {
                    pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);
                    pl.Add(new RunPayloadModel());
                }

                StringBuilder sbScript = new StringBuilder();

                sbScript.Append("Set-ADUser silcamner -Add @{msExchHideFromAddressLists=\"TRUE\"}");

                finalScript = sbScript.ToString();

                String result = util.RunPSScript(finalScript);

                return new ItemRunResult { ResultID = 2, ResultText = String.Format("The user was hidden from the Exchange address lists."), TimeDone = DateTime.Now, RunPayload = jsonPL };
            }
            catch (Exception ex)
            {
                return new ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}... Script Run: {1}", ex.Message, finalScript), TimeDone = DateTime.Now, RunPayload = jsonPL };
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