using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder;

namespace Microsoft.BotBuilderSamples.Dialogs.Permits
{
    public class PermitsDialog
    {
        private readonly string[] _permitOptions = { "Search Permit", "Download Permit", "Scrutiny Report" };

        public async Task<DialogTurnResult> ShowPermitOptionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;
            stepContext.Values["selectionMade"] = selection;

            if (selection == "Inspections" || selection == "Guides / Manuals")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Not Implementation yet."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var permitOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please help me in understanding with your choice from below."),
                Choices = ChoiceFactory.ToChoices(_permitOptions.ToList()),
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), permitOptions, cancellationToken);
        }
    }

}
