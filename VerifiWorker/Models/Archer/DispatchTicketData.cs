using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifiWorker.Models.Archer
{
    public class DispatchTicketData
    {
        public string TruckNumber { get; set; } = null!;
        public string DriverNumber { get; set; } = null!;
        public string DriverName { get; set; } = null!;
        public string DeliveryAddress1 { get; set; } = null!;
        public string DeliveryAddress2 { get; set; } = null!;
        public string CustomerNubmer { get; set; } = null!;
        public string MixCode { get; set; } = null!;
        public string OrderSize { get; set; } = null!;
        public string TargetSlump { get; set; } = null!;
        public string Uom { get; set; } = null!;
        public string Project { get; set; } = null!;
        public string OrderNumber { get; set; } = null!;
        public string PlantNumber { get; set; } = null!;
    }
}
