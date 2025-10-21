using System;
using System.Net.Http;
using System.Threading.Tasks;
using RHF_Foundation.Services.Interfaces;


namespace RHF_Foundation.Services.APIs
{

    public class CheckInAPIService : ICheckInAPIService
    {
        private readonly HttpClient _httpClient;

        private string _url = "https://api.thingspeak.com/update?api_key=62C8WYUK58D5MVN1&field4="; // Replace with your actual URL

        public CheckInAPIService(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> CheckInNumberOfVisitors(int numberAttending)
        {
            return await CheckIntoApi(4, numberAttending);
        }

        public async Task<string> CheckInArrival()
        {
            return await CheckIntoApi(5, 1);
        }

        private async Task<string> CheckIntoApi(int fieldNumber, int numberAttending)
        {
            string url = _url.Replace("field4=", $"field{fieldNumber}={numberAttending}");
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                // Optional: log or handle specific exceptions
                throw new HttpRequestException($"Error calling Check-In API: {ex.Message}", ex);
                return "error";
            }
        }
    }
}