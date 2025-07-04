using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifiWorker
{
    public class ProcessorOptions
    {
        public bool MultiLane { get; set; }
        //Enables upload to new hub.
        public bool EnableNewHub { get; set; }
        //Enables upload to legacy hub.
        public bool EnableLegacy { get; set; }
        //Set to false for imperial.
        public bool IsMetric { get; set; }
        public bool IsRegionalPrefix { get; set; }
    }
}
