using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;


namespace Microsoft.BotBuilderSamples.Dialogs.DownloadPermit
{
    public class DownloadPermitDialog
    {
        private readonly ApiDialog _apiDialog;
        private const string IsDownloadPermit = "value-isDownloadPermit";

        public DownloadPermitDialog(ApiDialog apiDialog)
        {
            _apiDialog = apiDialog;
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

        //  public async Task<DialogTurnResult> ProcessPermitNumberStepAsync(
        //WaterfallStepContext stepContext,
        //CancellationToken cancellationToken)
        //  {
        //      if ((bool)stepContext.Values[IsDownloadPermit])
        //      {
        //          var userEnteredPermitNumber = (string)stepContext.Result;
        //          var jsonDocument = await _apiDialog.GetPermitDataAsync(userEnteredPermitNumber);

        //          string permitType = jsonDocument.RootElement.GetProperty("permittype").GetString();
        //          string permitStatus = jsonDocument.RootElement.GetProperty("status").GetString();
        //          string sPDFURL = jsonDocument.RootElement.GetProperty("sPDFURL").GetString();

        //          // Create the view and download links with the actual API URL
        //          var viewLink = $"[View PDF]({sPDFURL})";
        //          var downloadLink = $"[Download PDF]({sPDFURL})";

        //          var formattedResponse = $"**Permit Number: {userEnteredPermitNumber}**\n\n" +
        //                                  $"Permit Type: {permitType}\n\n" +
        //                                  $"Status: {permitStatus}\n\n" +
        //                                  $"{viewLink} / {downloadLink}\n\n";

        //          await stepContext.Context.SendActivityAsync(MessageFactory.Text(formattedResponse), cancellationToken);
        //      }
        //      return await stepContext.NextAsync(null, cancellationToken);
        //  }
        public async Task<DialogTurnResult> ProcessPermitNumberStepAsync(
    WaterfallStepContext stepContext,
    CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Values[IsDownloadPermit])
            {
                var userEnteredPermitNumber = (string)stepContext.Result;
                var jsonDocument = await _apiDialog.GetPermitDataAsync(userEnteredPermitNumber);

                string permitType = jsonDocument.RootElement.GetProperty("permittype").GetString();
                string permitStatus = jsonDocument.RootElement.GetProperty("status").GetString();
                string sPDFURL = jsonDocument.RootElement.GetProperty("sPDFURL").GetString();

                // Generate the Adaptive Card JSON
                var adaptiveCardJson = AdaptiveCardGenerator.CreatePermitAdaptiveCard(userEnteredPermitNumber, permitType, permitStatus, sPDFURL);

                var adaptiveCard = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = adaptiveCardJson
                };

                var reply = MessageFactory.Attachment(adaptiveCard);
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

    }
}
