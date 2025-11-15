using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RHF_Foundation.Services.Interfaces; // adjust namespace

namespace RHF_Foundation.Services.APIs
{
    public class AzureCheckInAPI : ICheckInAPIService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "https://rhfwebapp-akahd8d0e6h9aefp.eastus2-01.azurewebsites.net/api/test/checkins";

        public AzureCheckInAPI(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        // POST: /single
        public async Task<string> CheckInArrival()
        {
            var url = $"{BaseUrl}/single";

            using var response = await _httpClient.PostAsync(url, null);
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        // POST: /bulk  body: { "count": numberAttending }
        public async Task<string> CheckInNumberOfVisitors(int numberAttending)
        {
            var url = $"{BaseUrl}/bulk";

            var payload = new
            {
                count = numberAttending
            };

            var json = JsonSerializer.Serialize(payload);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content);

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}
