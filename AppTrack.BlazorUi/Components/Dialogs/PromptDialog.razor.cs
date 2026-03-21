using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class PromptDialog
{
    [Inject] private IModelValidator<PromptModel> ModelValidator { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    /// <summary>
    /// When set, the dialog operates in edit mode for an existing prompt.
    /// When null, the dialog creates a new prompt.
    /// </summary>
    [Parameter] public PromptModel? ExistingPrompt { get; set; }

    /// <summary>
    /// The sibling collection used for uniqueness validation.
    /// </summary>
    [Parameter] public IEnumerable<PromptModel>? SiblingPrompts { get; set; }

    private PromptModel _model = new();
    private bool _isEdit;

    protected override void OnParametersSet()
    {
        _isEdit = ExistingPrompt is not null;

        if (_isEdit)
        {
            _model = new PromptModel
            {
                Id = ExistingPrompt!.Id,
                Name = ExistingPrompt.Name,
                PromptTemplate = ExistingPrompt.PromptTemplate,
                TempId = ExistingPrompt.TempId,
                CreationDate = ExistingPrompt.CreationDate,
                ModifiedDate = ExistingPrompt.ModifiedDate,
            };
        }
        else
        {
            _model = new PromptModel();
        }

        // Wire sibling collection for uniqueness validation
        _model.SiblingPrompts = SiblingPrompts;
    }

    private void OnNameChanged(string value)
    {
        _model.Name = value;
        ModelValidator.ResetErrors(nameof(PromptModel.Name));
    }

    private void OnPromptTemplateChanged(string value)
    {
        _model.PromptTemplate = value;
        ModelValidator.ResetErrors(nameof(PromptModel.PromptTemplate));
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private void Submit()
    {
        if (!ModelValidator.Validate(_model)) return;
        MudDialog.Close(DialogResult.Ok(_model));
    }

    private void Cancel() => MudDialog.Cancel();
}
