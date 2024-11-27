using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifiWorker.Models.Verifi
{
    public class VerifiLoadSummary
    {
        public string admixAtArrivalText { get; set; } = null!;
        public string admixAtDischargeText { get; set; } = null!;
        public string admixAtLeavePlantText { get; set; } = null!;
        public string ageAtDischargeMinutesText { get; set; } = null!;
        public string ageAtLeavePlantMinutesText { get; set; } = null!;
        public string ageAtReturnToPlantMinutes { get; set; } = null!;
        public int driverId { get; set; }
        public string driverName { get; set; } = null!;
        public string driverBatchSystemCode { get; set; } = null!;
        public string instructionName { get; set; } = null!;
        public double loadSize { get; set; }
        public string loadSizeText { get; set; } = null!;
        public string loadStartDateText { get; set; } = null!;
        public string loadStartLocationLocalTime { get; set; } = null!;
        public string loadStartTimeText { get; set; } = null!;
        public int locationId { get; set; }
        public string locationName { get; set; } = null!;
        public string maxAllowedWater { get; set; } = null!;
        public int mixCodeId { get; set; }
        public string mixCodeName { get; set; } = null!;
        public string mode { get; set; } = null!;
        public string orderNumber { get; set; } = null!;
        public int plantId { get; set; }
        public string slumpAtArrivalText { get; set; } = null!;
        public string slumpAtDischargeText { get; set; } = null!;
        public string slumpAtInitialSlumpText { get; set; } = null!;
        public string slumpAtLeavePlantText { get; set; } = null!;
        public double slumpFromTicket { get; set; }
        public string slumpFromTicketText { get; set; } = null!;
        public string slumpVsTargetAtArrivalText { get; set; } = null!;
        public string slumpVsTargetAtDischargeText { get; set; } = null!;
        public string slumpVsTargetAtInitialSlumpText { get; set; } = null!;
        public string targetSlumpAtArrival { get; set; } = null!;
        public string targetSlumpAtArrivalText { get; set; } = null!;
        public string targetSlumpAtDischarge { get; set; } = null!;
        public string targetSlumpAtDischargeText { get; set; } = null!;
        public string targetSlumpAtInitialSlump { get; set; } = null!;
        public string targetSlumpAtInitialSlumpText { get; set; } = null!;
        public string temperatureAtArrivalText { get; set; } = null!;
        public string temperatureAtDischargeText { get; set; } = null!;
        public string temperatureAtLeavePlantText { get; set; } = null!;
        public int ticketId { get; set; }
        public string ticketNumber { get; set; } = null!;
        public string totalRevsAtArrivalText { get; set; } = null!;
        public string totalRevsAtDischargeText { get; set; } = null!;
        public string totalRevsAtLeavePlantText { get; set; } = null!;
        public string totalRevsSinceLoadedAtArrivalText { get; set; } = null!;
        public string totalRevsSinceLoadedAtDischargeText { get; set; } = null!;
        public string totalRevsSinceLoadedAtLeavePlantText { get; set; } = null!;
        public int truckId { get; set; }
        public string truckMode { get; set; } = null!;
        public string truckName { get; set; } = null!;
        public string verifiWaterAtArrivalText { get; set; } = null!;
        public string verifiWaterAtDischargeText { get; set; } = null!;
        public string verifiWaterAtLeavePlantText { get; set; } = null!;
    }
}
