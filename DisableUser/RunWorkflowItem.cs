using System;
using System.Collections.Generic;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public ItemRunResult RunItem(int EmpID, String cnn, String RunPayload)
        {
            List<RunPayloadModel> pl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RunPayloadModel>>(RunPayload);

            pl.Add(new RunPayloadModel());

            String jsonPL = Newtonsoft.Json.JsonConvert.SerializeObject(pl);

            return new ItemRunResult { ResultID = 1, ResultText = "It worked", TimeDone = DateTime.Now, RunPayload = jsonPL };
        }
    }
}
