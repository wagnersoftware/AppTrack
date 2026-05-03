using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class ProjectMonitoring
{
    [Inject] private IProjectMonitoringService ProjectMonitoringService { get; set; } = null!;
    [Inject] private ISnackbarService SnackbarService { get; set; } = null!;

    private List<ProjectPortalDto> _portals = [];
    private List<string> _keywords = [];
    private bool _notifyByEmail;
    private int _notificationIntervalMinutes = 60;
    private string _newKeyword = string.Empty;
    private string _userEmail = string.Empty;
    private bool _isLoading;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        var portalsTask = ProjectMonitoringService.GetPortalsAsync();
        var settingsTask = ProjectMonitoringService.GetSettingsAsync();

        await Task.WhenAll(portalsTask, settingsTask);

        var portalsResponse = portalsTask.Result;
        var settingsResponse = settingsTask.Result;

        if (!SnackbarService.HandleResponse(portalsResponse) ||
            !SnackbarService.HandleResponse(settingsResponse))
        {
            _isLoading = false;
            return;
        }

        _portals = portalsResponse.Data is not null ? [.. portalsResponse.Data] : [];

        if (settingsResponse.Data is { } settings)
        {
            _keywords = settings.Keywords is not null ? [.. settings.Keywords] : [];
            _notifyByEmail = settings.NotifyByEmail;
            _notificationIntervalMinutes = settings.NotificationIntervalMinutes;
            _userEmail = settings.NotificationEmail ?? string.Empty;
        }

        _isLoading = false;
    }

    private void AddKeyword()
    {
        var kw = _newKeyword.Trim();
        if (string.IsNullOrWhiteSpace(kw) || _keywords.Contains(kw, StringComparer.OrdinalIgnoreCase))
            return;
        _keywords.Add(kw);
        _newKeyword = string.Empty;
    }

    private void OnKeywordKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            AddKeyword();
    }

    private void RemoveKeyword(string keyword) => _keywords.Remove(keyword);

    private async Task SubmitAsync()
    {
        _isBusy = true;

        var subscriptionsCommand = new SetPortalSubscriptionsCommand
        {
            Subscriptions = _portals.Select(p => new PortalSubscriptionItemDto
            {
                PortalId = p.Id,
                IsActive = p.IsSubscribed,
            }).ToList(),
        };

        var settingsCommand = new UpdateProjectMonitoringSettingsCommand
        {
            Keywords = _keywords,
            NotifyByEmail = _notifyByEmail,
            NotificationIntervalMinutes = _notificationIntervalMinutes,
        };

        var subscriptionsTask = ProjectMonitoringService.SetSubscriptionsAsync(subscriptionsCommand);
        var settingsTask = ProjectMonitoringService.UpdateSettingsAsync(settingsCommand);

        await Task.WhenAll(subscriptionsTask, settingsTask);

        _isBusy = false;
        await InvokeAsync(StateHasChanged);

        if (!SnackbarService.HandleResponse(subscriptionsTask.Result) ||
            !SnackbarService.HandleResponse(settingsTask.Result))
            return;

        SnackbarService.ShowSuccess("Project monitoring settings saved successfully.");
    }
}
