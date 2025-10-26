using AppTrack.Frontend.ApiService.ApiAuthenticationProvider;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.WpfUi.Contracts;
using AppTrack.WpfUi.MessageBoxService;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace AppTrack.WpfUi.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly IJobApplicationDefaultsService _jobApplicationDefaultsService;
    private readonly IWindowService _windowService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAiSettingsService _aiSettingsService;
    private readonly IApplicationTextService _applicationTextService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ApiAuthenticationStateProvider _apiAuthenticationStateProvider;
    private readonly IUserHelper _userHelper;

    //Binding for Datagrid to enable filtering
    public ICollectionView JobApplicationsView { get; set; }

    
    public ObservableCollection<JobApplicationModel> JobApplications { get; set; }

    [ObservableProperty]
    private bool isLoggedIn = false;

    [ObservableProperty]
    private string loggedInUser = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private JobApplicationModel.JobApplicationStatus? selectedStatus;

    [ObservableProperty]
    private IEnumerable<StatusOption> availableStatuses;

    private CancellationTokenSource? _cts;

    public MainViewModel(IJobApplicationService jobApplicationService,
                         IJobApplicationDefaultsService jobApplicationDefaultsService,
                         IWindowService windowService,
                         IServiceProvider serviceProvider,
                         IAiSettingsService aiSettingsService,
                         IApplicationTextService applicationTextService,
                         IMessageBoxService messageBoxService,
                         IAuthenticationService authenticationService,
                         ApiAuthenticationStateProvider apiAuthenticationStateProvider,
                         IUserHelper userHelper)
    {
        this._jobApplicationService = jobApplicationService;
        this._jobApplicationDefaultsService = jobApplicationDefaultsService;
        this._windowService = windowService;
        this._serviceProvider = serviceProvider;
        this._aiSettingsService = aiSettingsService;
        this._applicationTextService = applicationTextService;
        this._messageBoxService = messageBoxService;
        this._authenticationService = authenticationService;
        this._apiAuthenticationStateProvider = apiAuthenticationStateProvider;
        this._userHelper = userHelper;

        _apiAuthenticationStateProvider.AuthenticationStateChanged += ApiAuthenticationStateProvider_AuthenticationStateChanged;

        JobApplications = new ObservableCollection<JobApplicationModel>();
        JobApplicationsView = CollectionViewSource.GetDefaultView(JobApplications);
        JobApplicationsView.Filter = FilterByStatus;

        // Set up available statuses for filtering
        AvailableStatuses = new List<StatusOption>
            {
                new StatusOption { Status = null, DisplayName = "All" }
            }
        .Concat(Enum.GetValues(typeof(JobApplicationModel.JobApplicationStatus))
                 .Cast<JobApplicationModel.JobApplicationStatus>()
                 .Select(s => new StatusOption { Status = s, DisplayName = s.ToString() }))
        .ToList();
    }

    public async Task LoadJobApplicationsForUserAsync()
    {
        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        var apiResponse = await _jobApplicationService.GetJobApplicationsForUserAsync(userId);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
            return;
        }

        JobApplications.Clear();
        foreach (var x in apiResponse.Data ?? [])
            JobApplications.Add(x);

        JobApplicationsView.Refresh();
    }

    /// <summary>
    /// Determines whether the specified object represents a job application with a status matching the selected status
    /// filter.
    /// </summary>
    /// <remarks>If <see cref="SelectedStatus"/> is null, all job applications are included regardless of
    /// their status. Objects that are not of type <see cref="JobApplicationModel"/> are always excluded.</remarks>
    /// <param name="obj">The object to evaluate. Must be a <see cref="JobApplicationModel"/> instance to be considered for filtering.</param>
    /// <returns>true if the object is a <see cref="JobApplicationModel"/> and its status matches the selected status filter, or
    /// if no status filter is selected; otherwise, false.</returns>
    private bool FilterByStatus(object obj)
    {
        if (obj is not JobApplicationModel app)
            return false;

        if (SelectedStatus == null) // show all
            return true;

        return app.Status == SelectedStatus;
    }

    /// <summary>
    /// Handles changes to the selected job application status and refreshes the job applications view accordingly.
    /// </summary>
    /// <param name="value">The newly selected job application status. Can be null to indicate no status is selected.</param>
    partial void OnSelectedStatusChanged(JobApplicationModel.JobApplicationStatus? value)
    {
        JobApplicationsView?.Refresh();
    }

    private async void ApiAuthenticationStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var state = await task;
        LoggedInUser = state.User.Identity?.Name ?? string.Empty;
        IsLoggedIn = state.User.Identity?.IsAuthenticated ?? false;

        if (IsLoggedIn == false)
        {
            LoggedInUser = string.Empty;
            JobApplications.Clear();
        }
    }

    [RelayCommand]
    private async Task CreateJobApplication()
    {

        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        var response = await _jobApplicationDefaultsService.GetForUserAsync(userId);

        if (response.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(response);
            return;
        }

        var createJobApplicationViewModel = _serviceProvider.GetRequiredService<CreateJobApplicationViewModel>();
        createJobApplicationViewModel.SetDefaults(response.Data!);

        var windowResult = _windowService.ShowWindow(createJobApplicationViewModel);

        if (windowResult == false)
        {
            return;
        }

        var apiResponse = await _jobApplicationService.CreateJobApplicationForUserAsync(createJobApplicationViewModel.Model, userId);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
            return;
        }

        JobApplications.Add(apiResponse.Data!);

    }

    [RelayCommand]
    private async Task DeleteJobApplication(int id)
    {
        var dialogResult = _messageBoxService.ShowQuestionMessageBox($"Do you really want to delete application with id: {id}", "Delete application");

        if (dialogResult == MessageBoxResult.No || dialogResult == MessageBoxResult.Cancel)
        {
            return;
        }

        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        var apiResponse = await _jobApplicationService.DeleteJobApplicationAsync(id, userId);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
            return;
        }

        var jobApplicationToRemove = JobApplications.SingleOrDefault(x => x.Id == id);

        if (jobApplicationToRemove != null)
        {
            JobApplications.Remove(jobApplicationToRemove);
        }

    }

    [RelayCommand]
    private async Task EditJobApplication(JobApplicationModel jobApplicationModel)
    {
        var editJobApplicationViewModel = ActivatorUtilities.CreateInstance<EditJobApplicationViewModel>(_serviceProvider, jobApplicationModel);
        var windowResult = _windowService.ShowWindow(editJobApplicationViewModel);

        if (windowResult == false)
        {
            return;
        }

        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        var apiResponse = await _jobApplicationService.UpdateJobApplicationAsync(jobApplicationModel.Id, userId, jobApplicationModel);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
            return;
        }

        int index = JobApplications.IndexOf(jobApplicationModel);
        JobApplications.RemoveAt(index);
        JobApplications.Insert(index, apiResponse.Data!);

    }

    [RelayCommand]
    private async Task GenerateApplicationText(JobApplicationModel jobApplicationModel)
    {
        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        try
        {
            _cts = new CancellationTokenSource();

            IsLoading = true;
            var generatedPromptResponse = await _applicationTextService.GeneratePrompt(jobApplicationModel.Id, userId);
            IsLoading = false;

            if (generatedPromptResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(generatedPromptResponse);
                return;
            }

            var generatedPromptViewModel = ActivatorUtilities.CreateInstance<GeneratedPromptViewModel>(_serviceProvider, generatedPromptResponse.Data!);
            var promptDialogResult = _windowService.ShowWindow(generatedPromptViewModel);

            if (promptDialogResult == false)
            {
                return;
            }

            IsLoading = true;
            var applicationTextResponse = await _applicationTextService.GenerateApplicationText(generatedPromptViewModel.Text, userId, jobApplicationModel.Id, _cts.Token);
            IsLoading = false;

            if (applicationTextResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(applicationTextResponse);
                return;
            }

            jobApplicationModel.ApplicationText = applicationTextResponse.Data!.Text;

            var textViewModel = ActivatorUtilities.CreateInstance<ApplicationTextViewModel>(_serviceProvider, applicationTextResponse.Data);
            _windowService.ShowWindow(textViewModel);
        }
        finally
        {
            IsLoading = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private async Task SetDefaults()
    {
        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        var apiResponse = await _jobApplicationDefaultsService.GetForUserAsync(userId);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
            return;
        }

        var jobApplicatiobDefaultsViewModel = ActivatorUtilities.CreateInstance<SetJobApplicationDefaultsViewModel>(_serviceProvider, apiResponse.Data!);
        var windowResult = _windowService.ShowWindow(jobApplicatiobDefaultsViewModel);

        if (windowResult == false)
        {
            return;
        }

        apiResponse = await _jobApplicationDefaultsService.UpdateAsync(apiResponse.Data!.Id, apiResponse.Data);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
        }
    }

    [RelayCommand]
    private async Task OpenAiSettings()
    {
        var userId = await _userHelper.TryGetUserIdAsync();

        if (userId == null)
        {
            return;
        }

        var apiResponse = await _aiSettingsService.GetForUserAsync(userId);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
            return;
        }

        var setAiSettingsViewModel = ActivatorUtilities.CreateInstance<SetAiSettingsViewModel>(_serviceProvider, apiResponse.Data!);
        var windowResult = _windowService.ShowWindow(setAiSettingsViewModel);

        if (windowResult == false)
        {
            return;
        }

        apiResponse = await _aiSettingsService.UpdateAsync(apiResponse.Data!.Id, apiResponse.Data);

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox(apiResponse);
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        var isLoginSuccessful = _windowService.ShowWindow(loginViewModel);

        if (isLoginSuccessful == true)
        {
            await LoadJobApplicationsForUserAsync();
        }
    }
    [RelayCommand]
    private void Logout()
    {
        _authenticationService.Logout();
    }

    [RelayCommand]
    private void CancelOperation()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}
