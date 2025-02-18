using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifiWorker
{
    public class VerifiConnectorLegacy
    {
        private readonly VerifiConnectorLegacyOptions _options;
        private readonly ILogger<VerifiConnectorLegacy> _logger;
        private readonly string _baseUrl = "https://portal.verificoncrete.com/rs/api/";
        private readonly HttpClient _httpClient;

        public VerifiConnectorLegacy(IOptions<VerifiConnectorLegacyOptions> options, ILogger<VerifiConnectorLegacy> logger)
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
                var response = await _httpClient.PostAsync("1.4/ticket", new StringContent(xml, Encoding.UTF8, "application/xml"));
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
