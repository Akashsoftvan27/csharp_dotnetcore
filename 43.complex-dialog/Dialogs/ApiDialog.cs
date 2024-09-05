using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class ApiDialog
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly string _token;

        public ApiDialog(HttpClient httpClient, string apiBaseUrl, string token)
        {
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl;
            _token = token;
        }

        private async Task<string> GetApiResponseAsync(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/{endpoint}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<JsonDocument> GetPermitDataAsync(string permitNumber)
        {
            var responseData = await GetApiResponseAsync($"GetFileData?Fileno={permitNumber}");
            return JsonDocument.Parse(responseData);
        }

        public async Task<IEnumerable<string>> GetFileStatusDetailsAsync()
        {
            var responseData = await GetApiResponseAsync("getTotalcount");
            var jsonDocument = JsonDocument.Parse(responseData);
            var counts = jsonDocument.RootElement.GetProperty("counts").EnumerateArray();

            return counts.Select(count =>
                $"Total {count.GetProperty("statusName").GetString()}: {count.GetProperty("statusCount").GetString()}"
            ).ToList();
        }

        public async Task<IEnumerable<string>> GetRecentPermitsByStatusCodeAsync(int statusCode)
        {
            var responseData = await GetApiResponseAsync($"GetRecentPermits?StatusCode={statusCode}");
            var jsonDocument = JsonDocument.Parse(responseData);

            return jsonDocument.RootElement.GetProperty("recentPermits").EnumerateArray()
                .Select(x => x.GetString())
                .ToList();
        }
    }
}
