using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VerifiWorker.Models.Verifi;

namespace VerifiWorker
{
    public class VerifiConnector
    {
        private readonly VerifiConnectorOptions _options;
        private readonly ILogger<VerifiConnector> _logger;
        private readonly string _baseUrl = "https://hub.verificoncrete.com/api/";
        private readonly HttpClient _httpClient;

        public VerifiConnector(IOptions<VerifiConnectorOptions> options, ILogger<VerifiConnector> logger)
        {
            _options = options.Value;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("verifi-user", _options.Login);
            _httpClient.DefaultRequestHeaders.Add("verifi-user-pass", _options.Password);
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<bool> SendXMLTicket(string xml)
        {
            string result = string.Empty;
            try
            {
                var response = await _httpClient.PostAsync("ticket/v1.0", new StringContent(xml, Encoding.UTF8, "application/xml"));
                result = await response.Content.ReadAsStringAsync();
                if (result.Contains("Saved") || result.Contains("Sent"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending ticket to verifi - " + ex.Message);
                _logger.LogError("Ticket XML:");
                _logger.LogError(xml);
            }

            _logger.LogError("Verifi ticket upload error: " + result);
            _logger.LogError("Ticket XML:");
            _logger.LogError(xml);
            return false;
        }

        

    }
}
