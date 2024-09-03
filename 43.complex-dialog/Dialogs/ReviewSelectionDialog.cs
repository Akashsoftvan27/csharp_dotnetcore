using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Text.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Microsoft.BotBuilderSamples
{
    public class ReviewSelectionDialog : ComponentDialog
    {
        private const string SelectionMade = "value-selectionMade";
        private const string PermitNumber = "value-permitNumber";
        private const string IsDownloadPermit = "value-isDownloadPermit";

        private readonly string[] _mainMenuOptions = new string[]
        {
            "Permits", "Inspections", "Guides / Manuals"
        };

        private readonly string[] _permitOptions = new string[]
        {
            "Search Permit", "Download Permit", "Scrutiny Report"
        };

        private readonly HttpClient _httpClient = new HttpClient();

        public ReviewSelectionDialog()
            : base(nameof(ReviewSelectionDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                MainMenuStepAsync,
                SubMenuStepAsync,
                HandlePermitOptionStepAsync,
                ProcessPermitNumberStepAsync,
                SuperSubMenuStepAsync,
                HandleFileStatusChoiceAsync
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> MainMenuStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Thanks for connecting me." + Environment.NewLine +
                "How may I help you today?" + Environment.NewLine +
                "Please choose any of the below options to proceed further"),
                Choices = ChoiceFactory.ToChoices(_mainMenuOptions.ToList()),
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> SubMenuStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;
            stepContext.Values[SelectionMade] = selection;

            var permitOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please help me in understanding with your choice from below."),
                Choices = ChoiceFactory.ToChoices(_permitOptions.ToList()),
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), permitOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> HandlePermitOptionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;

            if (selection == "Download Permit")
            {
                stepContext.Values[IsDownloadPermit] = true;
                return await stepContext.PromptAsync(nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Please enter your permit number:") },
                    cancellationToken);
            }
            else
            {
                stepContext.Values[IsDownloadPermit] = false;
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ProcessPermitNumberStepAsync(
     WaterfallStepContext stepContext,
     CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Values[IsDownloadPermit])
            {
                var permitNumber = (string)stepContext.Result;
                var permitApiUrl = $"https://bot.civitpermit.in/api/CivitPermit/GetFileData?Fileno=PER%2F{permitNumber}%2F2024";

                var request = new HttpRequestMessage(HttpMethod.Get, permitApiUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "b14d3h48ab14ca753159a4e4134e41a54e2pq23g53bbce3ea2317");

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Unauthorized access. Please check your credentials."), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseData);

                string permitNumbers = jsonDocument.RootElement.GetProperty("permitnumber").GetString();
                string permittypes = jsonDocument.RootElement.GetProperty("permittype").GetString();
                string permitstatus = jsonDocument.RootElement.GetProperty("status").GetString();
                string base64Pdf = jsonDocument.RootElement.GetProperty("sBase64").GetString();

                // Convert Base64 string to byte array
                byte[] pdfBytes = Convert.FromBase64String(base64Pdf);

                // Save the byte array as a PDF file
                var filePath = Path.Combine("wwwroot", $"PER_{permitNumber}_2024.pdf");
                await File.WriteAllBytesAsync(filePath, pdfBytes);

                // Create view and download links
                var viewLink = $"[View](http://localhost:3978/PER_{permitNumber}_2024.pdf)";
                var downloadLink = $"[Download](http://localhost:3978/PER_{permitNumber}_2024.pdf)";

                var formattedResponse = $"Permit Number: {permitNumbers}\n\n" +
                                        $"Permit Type: {permittypes}\n\n" +
                                        $"Status: {permitstatus}\n\n" +
                                        $"{viewLink} / {downloadLink}\n\n";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(formattedResponse), cancellationToken);
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }



        private async Task<DialogTurnResult> SuperSubMenuStepAsync(
    WaterfallStepContext stepContext,
    CancellationToken cancellationToken)
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
        private async Task<DialogTurnResult> HandleFileStatusChoiceAsync(
    WaterfallStepContext stepContext,
    CancellationToken cancellationToken)
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