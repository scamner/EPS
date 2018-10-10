using System;
using System.Collections.Generic;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public ItemRunResult RunItem(int EmpID, String cnn, String RunPayload)
        {
            List<RunPayloadModel> pl = new List<RunPayloadModel>();

            if (!String.IsNullOrEmpty(RunPayload))
            {
                pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);
                pl.Add(new RunPayloadModel());
            }

            System.Threading.Thread.Sleep(30000);

            String jsonPL = String.IsNullOrEmpty(RunPayload) ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(pl);

            return new ItemRunResult { ResultID = 1, ResultText = "It worked", TimeDone = DateTime.Now, RunPayload = jsonPL };
        }
    }
}
