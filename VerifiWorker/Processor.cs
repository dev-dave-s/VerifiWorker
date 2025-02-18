using Coravel.Invocable;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VerifiWorker.Models.Archer;
using VerifiWorker.Models.Processor;

namespace VerifiWorker
{
    public class Processor : IInvocable
    {
        private readonly ProcessorOptions _options;
        private readonly ArcherConnector _archerConnector;
        private readonly VerifiConnector _verifiConnector;
        private readonly VerifiConnectorLegacy _verifiConnectorLegacy;
        private readonly TicketTracker _ticketTracker;
        private readonly ILogger<Processor> _logger;

        public Processor(IOptions<ProcessorOptions> options,
            ArcherConnector archerConnector,
            VerifiConnector verifiConnector,
            VerifiConnectorLegacy verifiConnectorLegacy,
            ILogger<Processor> logger,
            TicketTracker ticketTracker)
        {
            _options = options.Value;
            _archerConnector = archerConnector;
            _verifiConnector = verifiConnector;
            _verifiConnectorLegacy = verifiConnectorLegacy;
            _logger = logger;
            _ticketTracker = ticketTracker;
        }

        public async Task Invoke()
        {
            var results = await _archerConnector.GetLatestResultsAsync(DateTime.Now.AddHours(-1).ToUniversalTime());
            var tickets = results.Select(x => x.TicketId).Distinct().ToList();
            foreach(var ticket in tickets)
            {
                //var test = _ticketTracker.IsProcessed(ticket);
                if (!_ticketTracker.IsProcessed(ticket))
                {
                    var ticketResults = results.Where(x => x.TicketId == ticket).ToList();
                    var xml = SerializeXML(ticketResults);
                    if(await _verifiConnector.SendXMLTicket(xml))
                    {
                        _logger.LogInformation($"Ticket: {ticket} processing completed.");
                        _ticketTracker.AddTicket(ticket);
                    }
                    else 
                    {
                        _logger.LogError($"Unable to send ticket: {ticket} to verifi.");
                    }
                   
                }
                else
                {
                    _logger.LogDebug($"Skipping - Ticket: {ticket} Already Processed");
                }

                if (!_ticketTracker.IsLegacyProcessed(ticket) && _options.EnableLegacy)
                {
                    var ticketResults = results.Where(x => x.TicketId == ticket).ToList();

                    var legacyXml = SerializeXML(ticketResults, true);
                    if (await _verifiConnectorLegacy.SendXMLTicket(legacyXml))
                    {
                        _logger.LogInformation($"Ticket: {ticket} legacy connector processing completed.");
                        _ticketTracker.AddLegacyTicket(ticket);
                    }
                    else
                    {
                        _logger.LogError($"Unable to send ticket: {ticket} to verifi legacy connector.");
                    }
                }
                else
                {
                    _logger.LogDebug($"Skipping - (Legacy API) Ticket: {ticket} Already Processed");
                }
            }


        }

        public string SerializeXML(List<ArcherResult> ticketData, bool isLegacy = false)
        {
            var ticketUrl = "https://portal.verificoncrete.com/api/1.3/xml/ticket";
            var typeUrl = "https://portal.verificoncrete.com/api/1.3/xml/common-types";

            if (isLegacy)
            {
                ticketUrl = "https://portal.verificoncrete.com/api/1.4/xml/ticket";
                typeUrl = "https://portal.verificoncrete.com/api/1.4/xml/common-types";
            }

            var header = ticketData.FirstOrDefault();
            DispatchTicketData? dispatchTicket = null;
            if (header != null)
            {
                dispatchTicket = ExtractDispatchTicketData(header.DispatchTicket);
            }
            else
            {
                throw new Exception("Can't serialize without a header.");
            }

            XNamespace nameSpace1 = ticketUrl;
            XNamespace nameSpace2 = typeUrl;
            XName name1 = nameSpace1 + "verifi-ticket";

            XElement rootElement = new XElement(name1, new XAttribute("xmlns", ticketUrl),
                new XAttribute(XNamespace.Xmlns + "ns2", typeUrl),
                new XAttribute("ticket-id", header.TicketId),
                new XAttribute("order-id", dispatchTicket.OrderNumber),
                new XAttribute("created", header.EndTimeUTC.ToLocalTime()));

            XElement customerContent = new XElement(nameSpace1 + "customer",
                new XAttribute("source-system-id", dispatchTicket.CustomerNubmer), header.Customer);

            XElement jobContent = new XElement(nameSpace1 + "job",
                new XElement(nameSpace1 + "name", dispatchTicket.Project),
                new XElement(nameSpace1 + "address",
                new XElement(nameSpace2 + "address-line", dispatchTicket.DeliveryAddress1),
                new XElement(nameSpace2 + "address-line", dispatchTicket.DeliveryAddress2),
                new XElement(nameSpace2 + "city", string.Empty),
                new XElement(nameSpace2 + "state", string.Empty),
                new XElement(nameSpace2 + "postal-code", string.Empty)),
                new XElement(nameSpace1 + "volume-per-hour", "0",
                    new XAttribute("units", "m3")),
                new XElement(nameSpace1 + "order-size", dispatchTicket.OrderSize,
                    new XAttribute("units", dispatchTicket.Uom)));

            List<XAttribute> plantAttributes = new List<XAttribute>();

            plantAttributes.Add(new XAttribute("source-system-id", dispatchTicket.PlantNumber));
            //Only needed for multiple lanes.
            if (_options.MultiLane)
            {
                plantAttributes.Add(new XAttribute("sequence", header.DestinationId));
            }

            XElement plantContent = new XElement(nameSpace1 + "plant", plantAttributes);

            XElement truckContent = new XElement(nameSpace1 + "truck", dispatchTicket.TruckNumber);

            XElement driverNameContent = new XElement(nameSpace1 + "driver",
                new XAttribute("source-system-id", dispatchTicket.DriverNumber), dispatchTicket.DriverName);

            XElement mixContent = new XElement(nameSpace1 + "mix",
                new XElement(nameSpace1 + "mix-code", dispatchTicket.MixCode));

            XElement loadsizeContent = new XElement(nameSpace1 + "load-size",
                new XAttribute("units", GetArcherValue(header.LoadSize, _options.IsMetric).Uom), GetArcherValue(header.LoadSize, _options.IsMetric).Value);

            //*** note slump units is currently hardcoded to mm.
            XElement slumpContent = new XElement(nameSpace1 + "target-slump",
                new XAttribute("units", "mm"), new XElement(nameSpace2 + "ordered", dispatchTicket.TargetSlump));

            XElement batchStartContent = new XElement(nameSpace1 + "batch-start", header.StartTimeUTC.ToLocalTime());

            XElement batchFinishContent = new XElement(nameSpace1 + "batch-finish-time", header.EndTimeUTC.ToLocalTime());

            XElement allowedWaterContent = new XElement(nameSpace1 + "net-allowable-water",
                new XAttribute("units", GetArcherValue(header.AllowedWater, _options.IsMetric).Uom), GetArcherValue(header.AllowedWater, _options.IsMetric).Value);

            XElement materialMainContent = new XElement(nameSpace1 + "materials");
            foreach (var item in ticketData)
            {
                materialMainContent.Add(new XElement(nameSpace1 + "material",
                    new XElement(nameSpace1 + "description", item.DispatchId),
                    new XElement(nameSpace1 + "target-weight", GetArcherValue(item.Target, _options.IsMetric).Value,
                        new XAttribute("units", GetArcherValue(item.Target, _options.IsMetric).Uom)),
                    new XElement(nameSpace1 + "measured-weight", GetArcherValue(item.Actual).Value,
                        new XAttribute("units", GetArcherValue(item.Actual, _options.IsMetric).Uom)),
                    new XElement(nameSpace1 + "apparent-moisture-percentage", header.Moisture),
                    new XElement(nameSpace1 + "absorption-capacity", item.Absorption),
                    //*** note specific gravity is currently hard coded to 1.
                    new XElement(nameSpace1 + "specific-gravity", "1")));

            }

            rootElement.Add(customerContent);
            rootElement.Add(jobContent);
            rootElement.Add(plantContent);
            rootElement.Add(truckContent);
            rootElement.Add(driverNameContent);
            rootElement.Add(mixContent);
            rootElement.Add(loadsizeContent);
            rootElement.Add(slumpContent);
            rootElement.Add(batchStartContent);
            rootElement.Add(batchFinishContent);
            rootElement.Add(allowedWaterContent);
            rootElement.Add(materialMainContent);

            return rootElement.ToString(SaveOptions.DisableFormatting);
        }

        public DispatchTicketData ExtractDispatchTicketData(string dispatchTicket)
        {
            var result = new DispatchTicketData();

            var dispatchTicketData = new List<DispatchTicketItem>();

            var values = dispatchTicket.Split("\t");
            for(int i = 0; i < values.Length; i++)
            {
                var test = values[i].Substring(0, values[i].IndexOf("="));
                var name = values[i].Substring(values[i].IndexOf("=")+1);
                dispatchTicketData.Add(new DispatchTicketItem
                {
                    Id = int.Parse(values[i].Substring(0, values[i].IndexOf("="))),
                    Value = values[i].Substring(values[i].IndexOf("=") + 1)
                });
            }



            //Mix Code 5

            result.MixCode = ExtractField(5, dispatchTicketData);
            //Ordered Slump 7
            result.TargetSlump = ExtractField(7, dispatchTicketData);
            //Driver Number 14
            result.DriverNumber = ExtractField(14, dispatchTicketData);
            //Driver Name 15
            result.DriverName = ExtractField(15, dispatchTicketData);
            //Delivery Address1 16
            result.DeliveryAddress1 = ExtractField(16, dispatchTicketData);
            //Delivery Address2 17
            result.DeliveryAddress2 = ExtractField(17, dispatchTicketData);
            //Customer Number 181
            result.CustomerNubmer = ExtractField(181, dispatchTicketData);
            //Order Size 28
            result.OrderSize = ExtractField(28, dispatchTicketData);

            result.Uom = ExtractField(31, dispatchTicketData);
            //Truck Number
            result.TruckNumber = ExtractField(3, dispatchTicketData);

            result.Project = ExtractField(188, dispatchTicketData);

            result.OrderNumber = ExtractField(19, dispatchTicketData);

            result.PlantNumber = ExtractField(1, dispatchTicketData);

            return result;
        }

        public ArcherValue GetArcherValue(string value, bool isMetric = true)
        {
            if (isMetric)
            {
                ArcherValue result = new ArcherValue();
                if (value.Contains(" l") || value.Contains("l"))
                {
                    var amount = value.Substring(0, value.IndexOf("l")).Trim();
                    var unit = value.Substring(value.IndexOf("l"), 1);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                if (value.Contains("kg"))
                {
                    var amount = value.Substring(0, value.IndexOf("k")).Trim();
                    var unit = value.Substring(value.IndexOf("k"), 2);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                if (value.Contains("m3"))
                {
                    var amount = value.Substring(0, value.IndexOf("m")).Trim();
                    var unit = value.Substring(value.IndexOf("m"), 2);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                return result;
            }
            else
            {
                ArcherValue result = new ArcherValue();
                //gal
                if (value.Contains("g"))
                {
                    var amount = value.Substring(0, value.IndexOf("g")).Trim();
                    var unit = value.Substring(value.IndexOf("g"), 3);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                //in
                if (value.Contains("in"))
                {
                    var amount = value.Substring(0, value.IndexOf("i")).Trim();
                    var unit = value.Substring(value.IndexOf("i"), 2);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                //lb
                if (value.Contains("lb"))
                {
                    var amount = value.Substring(0, value.IndexOf("l")).Trim();
                    var unit = value.Substring(value.IndexOf("l"), 2);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                //y3
                if (value.Contains("y3"))
                {
                    var amount = value.Substring(0, value.IndexOf("y")).Trim();
                    var unit = value.Substring(value.IndexOf("y"), 2);
                    result.Value = Math.Round(decimal.Parse(amount), 4);
                    result.Uom = unit;
                }

                return result;
            }
        }

        public string ExtractField(int fieldNumber, List<DispatchTicketItem> Data)
        {
            var search = Data.FirstOrDefault(x => x.Id == fieldNumber);
            if(search == null)
            {
                return string.Empty;
            }

            return search.Value;
        }

    }
}
