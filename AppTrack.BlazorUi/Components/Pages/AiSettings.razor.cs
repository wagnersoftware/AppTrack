using AppTrack.BlazorUi.Components.Dialogs;
using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class AiSettings
{
    [Inject] private IAiSettingsService AiSettingsService { get; set; } = null!;
    [Inject] private IChatModelsService ChatModelsService { get; set; } = null!;
    [Inject] private IModelValidator<AiSettingsModel> ModelValidator { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private static readonly DialogOptions _paramDialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
    };

    private AiSettingsModel _model = new();
    private List<ChatModel> _chatModels = [];
    private ChatModel? _selectedChatModel;
    private bool _isLoading;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        var settingsTask = AiSettingsService.GetForUserAsync();
        var chatModelsTask = ChatModelsService.GetChatModelsAsync();

        await Task.WhenAll(settingsTask, chatModelsTask);

        var settingsResponse = settingsTask.Result;
        var chatModelsResponse = chatModelsTask.Result;

        if (!ErrorHandlingService.HandleResponse(settingsResponse) ||
            !ErrorHandlingService.HandleResponse(chatModelsResponse))
        {
            _isLoading = false;
            return;
        }

        _model = settingsResponse.Data ?? new AiSettingsModel();
        _chatModels = chatModelsResponse.Data ?? [];

        if (_model.SelectedChatModelId == 0 && _chatModels.Count > 0)
            _model.SelectedChatModelId = _chatModels[0].Id;

        _selectedChatModel = _chatModels.FirstOrDefault(m => m.Id == _model.SelectedChatModelId);

        _isLoading = false;
    }

    private void OnChatModelChanged(ChatModel value)
    {
        _selectedChatModel = value;
    }

    private async Task AddPromptParameterAsync()
    {
        var parameters = new DialogParameters<PromptParameterDialog>
        {
            { x => x.SiblingParameters, _model.PromptParameter },
        };

        var dialog = await DialogService.ShowAsync<PromptParameterDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptParameterModel newParam) return;

        _model.PromptParameter.Add(newParam);
        await InvokeAsync(StateHasChanged);
    }

    private async Task EditPromptParameterAsync(PromptParameterModel param)
    {
        var parameters = new DialogParameters<PromptParameterDialog>
        {
            { x => x.ExistingParameter, param },
            { x => x.SiblingParameters, _model.PromptParameter },
        };

        var dialog = await DialogService.ShowAsync<PromptParameterDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptParameterModel updatedParam) return;

        param.Key = updatedParam.Key;
        param.Value = updatedParam.Value;

        await InvokeAsync(StateHasChanged);
    }

    private void DeletePromptParameter(PromptParameterModel param)
    {
        _model.PromptParameter.Remove(param);
    }

    private async Task AddPromptAsync()
    {
        var parameters = new DialogParameters<PromptDialog>
        {
            { x => x.SiblingPrompts, _model.Prompts },
        };

        var dialog = await DialogService.ShowAsync<PromptDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptModel newPrompt) return;

        _model.Prompts.Add(newPrompt);
        await InvokeAsync(StateHasChanged);
    }

    private async Task EditPromptAsync(PromptModel prompt)
    {
        var parameters = new DialogParameters<PromptDialog>
        {
            { x => x.ExistingPrompt, prompt },
            { x => x.SiblingPrompts, _model.Prompts },
        };

        var dialog = await DialogService.ShowAsync<PromptDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptModel updatedPrompt) return;

        prompt.Key = updatedPrompt.Key;
        prompt.PromptTemplate = updatedPrompt.PromptTemplate;

        await InvokeAsync(StateHasChanged);
    }

    private void DeletePrompt(PromptModel prompt)
    {
        _model.Prompts.Remove(prompt);
    }

    private async Task SubmitAsync()
    {
        _model.SelectedChatModelId = _selectedChatModel?.Id ?? _model.SelectedChatModelId;

        if (!ModelValidator.Validate(_model)) return;

        _isBusy = true;
        var response = await AiSettingsService.UpdateAsync(_model.Id, _model);
        _isBusy = false;
        await InvokeAsync(StateHasChanged);

        if (!ErrorHandlingService.HandleResponse(response)) return;

        ErrorHandlingService.ShowSuccess("AI settings saved successfully.");
    }
}
