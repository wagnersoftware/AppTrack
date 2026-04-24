using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class RssOptions
{
    [Inject] private IRssSettingsService RssSettingsService { get; set; } = null!;
    [Inject] private ISnackbarService SnackbarService { get; set; } = null!;

    private List<RssPortalModel> _portals = [];
    private RssMonitoringSettingsModel _settings = new();
    private string _newKeyword = string.Empty;
    private bool _isLoading;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        var portalsTask = RssSettingsService.GetPortalsAsync();
        var settingsTask = RssSettingsService.GetSettingsAsync();

        await Task.WhenAll(portalsTask, settingsTask);

        var portalsResponse = portalsTask.Result;
        var settingsResponse = settingsTask.Result;

        if (!SnackbarService.HandleResponse(portalsResponse) ||
            !SnackbarService.HandleResponse(settingsResponse))
        {
            _isLoading = false;
            return;
        }

        _portals = portalsResponse.Data ?? [];
        _settings = settingsResponse.Data ?? new RssMonitoringSettingsModel();

        _isLoading = false;
    }

    private void AddKeyword()
    {
        var kw = _newKeyword.Trim();
        if (string.IsNullOrWhiteSpace(kw) || _settings.Keywords.Contains(kw, StringComparer.OrdinalIgnoreCase))
            return;
        _settings.Keywords.Add(kw);
        _newKeyword = string.Empty;
    }

    private void OnKeywordKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            AddKeyword();
    }

    private void RemoveKeyword(string keyword) => _settings.Keywords.Remove(keyword);

    private async Task SubmitAsync()
    {
        _isBusy = true;

        var subscriptionsTask = RssSettingsService.SetSubscriptionsAsync(_portals);
        var settingsTask = RssSettingsService.UpdateSettingsAsync(_settings);

        await Task.WhenAll(subscriptionsTask, settingsTask);

        _isBusy = false;
        await InvokeAsync(StateHasChanged);

        if (!SnackbarService.HandleResponse(subscriptionsTask.Result) ||
            !SnackbarService.HandleResponse(settingsTask.Result))
            return;

        SnackbarService.ShowSuccess("RSS options saved successfully.");
    }
}
