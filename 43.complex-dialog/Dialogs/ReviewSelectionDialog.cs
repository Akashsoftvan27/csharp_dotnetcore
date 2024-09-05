using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples.Dialogs.DownloadPermit;
using Microsoft.BotBuilderSamples.Dialogs.Permits;
using Microsoft.BotBuilderSamples.Dialogs.SearchPermit;
using Microsoft.BotBuilderSamples.Dialogs.SelectPermit;
using Microsoft.BotBuilderSamples.Dialogs;

public class ReviewSelectionDialog : ComponentDialog
{
    private readonly SelectPermitDialog _selectPermitDialog;
    private readonly PermitsDialog _permitsDialog;
    private readonly DownloadPermitDialog _downloadPermitDialog;
    private readonly SearchPermitDialog _searchPermitDialog;

    public ReviewSelectionDialog(ApiDialog apiDialog)
        : base(nameof(ReviewSelectionDialog))
    {
        _selectPermitDialog = new SelectPermitDialog();
        _permitsDialog = new PermitsDialog();
        _downloadPermitDialog = new DownloadPermitDialog(apiDialog);
        _searchPermitDialog = new SearchPermitDialog(apiDialog);

        var waterfallSteps = new WaterfallStep[]
        {
            _selectPermitDialog.ShowMainMenuAsync,
            _permitsDialog.ShowPermitOptionsAsync,
            _downloadPermitDialog.HandlePermitOptionStepAsync,
            _downloadPermitDialog.ProcessPermitNumberStepAsync,
            _searchPermitDialog.ShowFileStatusAsync,
            _searchPermitDialog.HandleFileStatusChoiceAsync,
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

        InitialDialogId = nameof(WaterfallDialog);
    }
}
