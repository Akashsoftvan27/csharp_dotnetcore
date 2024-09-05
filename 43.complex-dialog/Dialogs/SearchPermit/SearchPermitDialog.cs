using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs.SearchPermit
{
    public class SearchPermitDialog
    {
        private readonly ApiDialog _apiDialog;

        public SearchPermitDialog(ApiDialog apiDialog)
        {
            _apiDialog = apiDialog;
        }

        public async Task<DialogTurnResult> ShowFileStatusAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var fileStatusDetails = await _apiDialog.GetFileStatusDetailsAsync();
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Status of your total files."),
                Choices = ChoiceFactory.ToChoices(fileStatusDetails.ToList()),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        public async Task<DialogTurnResult> HandleFileStatusChoiceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = ((FoundChoice)stepContext.Result).Value;
            IEnumerable<string> recentPermits = Enumerable.Empty<string>();

            switch (choice)
            {
                case var _ when choice.Contains("Applied"):
                    recentPermits = await _apiDialog.GetRecentPermitsByStatusCodeAsync(1);
                    break;
                case var _ when choice.Contains("Approved"):
                    recentPermits = await _apiDialog.GetRecentPermitsByStatusCodeAsync(3);
                    break;
                case var _ when choice.Contains("Rejected"):
                    recentPermits = await _apiDialog.GetRecentPermitsByStatusCodeAsync(2);
                    break;
            }

            var staticMessage = $"Your recent permits:\n\n{string.Join(Environment.NewLine, recentPermits)}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(staticMessage), cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}
