﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemToRun
{
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
