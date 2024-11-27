using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifiWorker.Models.Archer;

namespace VerifiWorker
{
    public class ArcherConnector
    {
        private readonly ArcherConnectorOptions _options;
        private readonly ILogger<ArcherConnector> _logger;

        public ArcherConnector(IOptions<ArcherConnectorOptions> options, ILogger<ArcherConnector> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<List<ArcherResult>> GetLatestResultsAsync(DateTime greaterThanDateUTC)
        {
            var result = new List<ArcherResult>();
            using (SqlConnection conn = new SqlConnection(_options.ConnectionString))
            {
                SqlCommand command = new SqlCommand(GetLatestResultsQuery(greaterThanDateUTC), conn);
                try
                {
                    conn.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var row = new ArcherResult
                            {
                                LoadKey = reader.GetInt32(0),
                                TicketId = reader.GetString(1),
                                LoadSize = reader.GetString(2),
                                Customer = reader.GetString(3),
                                DispatchId = reader.GetString(4),
                                StartTimeUTC = reader.GetDateTime(5),
                                EndTimeUTC = reader.GetDateTime(6),
                                AllowedWater = reader.GetString(7),
                                Actual = reader.GetString(8),
                                Target = reader.GetString(9),
                                Absorption = reader.GetDouble(10),
                                BatchDischargeTime = reader.GetDateTime(11),
                                DispatchTicket = reader.GetString(12),
                                Moisture = reader.GetDouble(13),
                                DestinationId = reader.GetString(14)
                            };

                            result.Add(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"Error getting data from SQL. Exception: {ex.Message}");
                }
            }

            return result;
        }

        private string GetLatestResultsQuery(DateTime greaterThanDateUTC)
        {
            return @"
                    SELECT 
                        dbo.Loads.[key],
                        dbo.Loads.TicketId,
                        dbo.Loads.[Size],
                        dbo.Loads.Customer,
                        dbo.Materials.DispatchId,
                        dbo.Loads.StartTime,
                        dbo.loads.EndTime,
                        dbo.loads.AllowedWater,
                        dbo.MaterialRequests.Actual, 
                        dbo.MaterialRequests.Target,
                        dbo.MaterialRequests.Absorption,
                        dbo.Loads.BatchDischargeTime, 
                        dbo.Loads.DispatchTicket,
                        dbo.MaterialRequests.Moisture,
                        dbo.Loads.DestinationId
                    FROM 
                        dbo.Loads 
                    INNER JOIN 
                        dbo.MaterialRequests 
                    ON 
                        ( 
                            dbo.Loads.[Key] = dbo.MaterialRequests.LoadKey) 
                    LEFT JOIN 
                        dbo.Jobs 
                    ON 
                        ( 
                            dbo.Loads.JobKey = dbo.Jobs.[Key]) 
                    LEFT JOIN 
                        dbo.Materials 
                    ON 
                        ( 
                            dbo.MaterialRequests.Materialkey = dbo.Materials.[Key]) 
                    WHERE 
                        dbo.Loads.BatchDischargeTime >= '" + greaterThanDateUTC.ToString("yyyy-MM-dd HH:mm:ss") + @"' and BatchResultReported = 1
                    Order By dbo.loads.TicketId
                    ";
        }
    }
}
