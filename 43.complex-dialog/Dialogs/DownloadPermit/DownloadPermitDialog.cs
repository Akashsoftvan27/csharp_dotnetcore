using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Net;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Dialogs.DownloadPermit
{
    public class DownloadPermitDialog
    {
        private readonly HttpClient _httpClient;
        private const string IsDownloadPermit = "value-isDownloadPermit";

        public DownloadPermitDialog(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DialogTurnResult> HandlePermitOptionStepAsync(
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
            else if (selection == "Scrutiny Report")
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Not Implementation yet."),
                    cancellationToken
                );
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                stepContext.Values[IsDownloadPermit] = false;
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
        public async Task<DialogTurnResult> ProcessPermitNumberStepAsync(
       WaterfallStepContext stepContext,
       CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Values[IsDownloadPermit])
            {
                var userEnteredPermitNumber = (string)stepContext.Result; // The permit number entered by the user
                var permitApiUrl = $"https://bot.civitpermit.in/api/CivitPermit/GetFileData?Fileno=PER%2F{userEnteredPermitNumber}%2F2024";

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

                string permitType = jsonDocument.RootElement.GetProperty("permittype").GetString();
                string permitStatus = jsonDocument.RootElement.GetProperty("status").GetString();
                string base64Pdf = jsonDocument.RootElement.GetProperty("sBase64").GetString();

                byte[] pdfBytes = Convert.FromBase64String(base64Pdf);
                var fileName = $"{userEnteredPermitNumber}.pdf";

                using (var memoryStream = new MemoryStream(pdfBytes))
                {
                    var attachment = new Attachment
                    {
                        ContentType = "application/pdf",
                        ContentUrl = "data:application/pdf;base64," + Convert.ToBase64String(pdfBytes),
                        Name = fileName
                    };

                    var reply = MessageFactory.Attachment(attachment);

                    reply.Text = $"Permit Number: {userEnteredPermitNumber}\n\n" +
                                  $"Permit Type: {permitType}\n\n" +
                                  $"Status: {permitStatus}\n\n";
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

    }
}
