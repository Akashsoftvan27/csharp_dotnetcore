using System.Collections.Generic;
using System.Linq;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs.SearchPermit
{
    public class SearchPermitDialog
    {
        private readonly HttpClient _httpClient;

        public SearchPermitDialog(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DialogTurnResult> ShowFileStatusAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var fileStatusDetails = await GetFileStatusDetailsAsync();
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Status of your total files."),
                Choices = ChoiceFactory.ToChoices(fileStatusDetails.ToList()),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<IEnumerable<string>> GetFileStatusDetailsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://bot.civitpermit.in/api/CivitPermit/getTotalcount");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "b14d3h48ab14ca753159a4e4134e41a54e2pq23g53bbce3ea2317");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(responseData);
            var counts = jsonDocument.RootElement.GetProperty("counts").EnumerateArray();

            var fileStatusDetails = counts.Select(count =>
                $"Total {count.GetProperty("statusName").GetString()}: {count.GetProperty("statusCount").GetString()}"
            ).ToList();

            return fileStatusDetails;
        }

        public async Task<DialogTurnResult> HandleFileStatusChoiceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = ((FoundChoice)stepContext.Result).Value;
            IEnumerable<string> recentPermits = Enumerable.Empty<string>();

            switch (choice)
            {
                case var _ when choice.Contains("Applied"):
                    recentPermits = await GetRecentPermitsByStatusCodeAsync(1);
                    break;
                case var _ when choice.Contains("Approved"):
                    recentPermits = await GetRecentPermitsByStatusCodeAsync(3);
                    break;
                case var _ when choice.Contains("Rejected"):
                    recentPermits = await GetRecentPermitsByStatusCodeAsync(2);
                    break;
            }

            var staticMessage = $"Your recent permits:\n\n{string.Join(Environment.NewLine, recentPermits)}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(staticMessage), cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<IEnumerable<string>> GetRecentPermitsByStatusCodeAsync(int statusCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://bot.civitpermit.in/api/CivitPermit/GetRecentPermits?StatusCode={statusCode}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "b14d3h48ab14ca753159a4e4134e41a54e2pq23g53bbce3ea2317");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseData);
            var permits = jsonDocument.RootElement.GetProperty("recentPermits").EnumerateArray()
                                  .Select(x => x.GetString())
                                  .ToList();

            return permits;
        }
    }

}
