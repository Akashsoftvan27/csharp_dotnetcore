using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using System;

namespace Microsoft.BotBuilderSamples.Dialogs.SelectPermit
{
    public class SelectPermitDialog
    {
        private readonly string[] _mainMenuOptions = { "Permits", "Inspections", "Guides / Manuals" };

        public async Task<DialogTurnResult> ShowMainMenuAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
    }

}
