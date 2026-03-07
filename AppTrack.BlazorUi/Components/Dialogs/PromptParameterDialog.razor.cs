using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class PromptParameterDialog
{
    [Inject] private IModelValidator<PromptParameterModel> ModelValidator { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    /// <summary>
    /// When set, the dialog operates in edit mode for an existing parameter.
    /// When null, the dialog creates a new parameter.
    /// </summary>
    [Parameter] public PromptParameterModel? ExistingParameter { get; set; }

    /// <summary>
    /// The sibling collection used for uniqueness validation.
    /// </summary>
    [Parameter] public IEnumerable<PromptParameterModel>? SiblingParameters { get; set; }

    private PromptParameterModel _model = new();
    private bool _isEdit;

    protected override void OnParametersSet()
    {
        _isEdit = ExistingParameter is not null;

        if (_isEdit)
        {
            _model = new PromptParameterModel
            {
                Id = ExistingParameter!.Id,
                Key = ExistingParameter.Key,
                Value = ExistingParameter.Value,
                TempId = ExistingParameter.TempId,
                CreationDate = ExistingParameter.CreationDate,
                ModifiedDate = ExistingParameter.ModifiedDate,
            };
        }
        else
        {
            _model = new PromptParameterModel();
        }

        // Wire the sibling collection so the uniqueness rule in PromptParameterModelValidator can run.
        _model.ParentCollection = SiblingParameters;
    }

    private void OnKeyChanged(string value)
    {
        _model.Key = value;
        ModelValidator.ResetErrors(nameof(PromptParameterModel.Key));
    }

    private void OnValueChanged(string value)
    {
        _model.Value = value;
        ModelValidator.ResetErrors(nameof(PromptParameterModel.Value));
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
