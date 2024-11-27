using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifiWorker.Models.Archer
{
    public class ArcherResult
    {
        public int LoadKey { get; set; }
        public string TicketId { get; set; } = null!;
        public string LoadSize { get; set; } = null!;
        public string Customer { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public DateTime StartTimeUTC { get; set; }
        public DateTime EndTimeUTC { get; set; }
        public string AllowedWater { get; set; } = null!;
        public string Actual { get; set; } = null!;
        public string Target { get; set; } = null!;
        public string TargetUom { get; set; } = null!;
        public double Absorption { get; set; }
        public DateTime BatchDischargeTime { get; set; }
        public string DispatchTicket { get; set; } = null!;
        public double Moisture { get; set; }
        public string DestinationId { get; set; } = null!;

    }

}
