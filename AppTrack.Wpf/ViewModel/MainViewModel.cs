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
using System.Windows;

namespace AppTrack.WpfUi.ViewModel
{
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

        public ObservableCollection<JobApplicationModel> JobApplications { get; set; } = new();

        [ObservableProperty]
        private bool isLoggedIn = false;

        [ObservableProperty]
        private string loggedInUser = string.Empty;

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
        }

        private async void ApiAuthenticationStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var state = await task;
            LoggedInUser = state.User.Identity?.Name ?? string.Empty;
            IsLoggedIn = state.User.Identity?.IsAuthenticated ?? false;
            
            if(IsLoggedIn == false)
            {
                LoggedInUser = string.Empty;
                JobApplications.Clear();
            }
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
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
            }

            apiResponse.Data?.ForEach(x => JobApplications.Add(x));
        }

        [RelayCommand]
        private async Task CreateJobApplication()
        {

            var userId = await _userHelper.TryGetUserIdAsync();

            if(userId == null)
            {
                return;
            }

            var response = await _jobApplicationDefaultsService.GetForUserAsync(userId);

            var createJobApplicationViewModel = _serviceProvider.GetRequiredService<CreateJobApplicationViewModel>();
            createJobApplicationViewModel.SetDefaults(response.Data);

            var windowResult = _windowService.ShowWindow(createJobApplicationViewModel);

            if (windowResult == false)
            {
                return;
            }

            var apiResponse = await _jobApplicationService.CreateJobApplicationForUserAsync(createJobApplicationViewModel.Model, userId);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
            }

            JobApplications.Add(apiResponse.Data);

        }

        [RelayCommand]
        private async Task DeleteJobApplication(int id)
        {
            var dialogResult = _messageBoxService.ShowQuestionMessageBox($"Do you really want to delete application with id: {id}", "Delete application");

            if (dialogResult == MessageBoxResult.No || dialogResult == MessageBoxResult.Cancel)
            {
                return;
            }

            var apiResponse = await _jobApplicationService.DeleteJobApplicationAsync(id);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
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

            var apiResponse = await _jobApplicationService.UpdateJobApplicationAsync(jobApplicationModel.Id, jobApplicationModel);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
                return;
            }

            int index = JobApplications.IndexOf(jobApplicationModel);
            JobApplications.RemoveAt(index);
            JobApplications.Insert(index, apiResponse.Data);

        }

        [RelayCommand]
        private async Task GenerateApplicationText(JobApplicationModel jobApplicationModel)
        {
            var userId = await _userHelper.TryGetUserIdAsync();

            if (userId == null)
            {
                return;
            }

            var apiResponse = await _applicationTextService.GenerateApplicationText(jobApplicationModel.Id, userId, jobApplicationModel.URL, jobApplicationModel.Position);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
                return;
            }

            jobApplicationModel.ApplicationText = apiResponse.Data;
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
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
            }

            var jobApplicatiobDefaultsViewModel = ActivatorUtilities.CreateInstance<SetJobApplicationDefaultsViewModel>(_serviceProvider, apiResponse.Data);
            var windowResult = _windowService.ShowWindow(jobApplicatiobDefaultsViewModel);

            if (windowResult == false)
            {
                return;
            }

            apiResponse = await _jobApplicationDefaultsService.UpdateAsync(apiResponse.Data.Id, apiResponse.Data);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
            }
        }

        [RelayCommand]
        private async Task AiSettings()
        {
            var userId = await _userHelper.TryGetUserIdAsync();

            if (userId == null)
            {
                return;
            }

            var apiResponse = await _aiSettingsService.GetForUserAsync(userId);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
            }

            var setAiSettingsViewModel = ActivatorUtilities.CreateInstance<SetAiSettingsViewModel>(_serviceProvider, apiResponse.Data);
            var windowResult = _windowService.ShowWindow(setAiSettingsViewModel);

            if (windowResult == false)
            {
                return;
            }

            apiResponse = await _aiSettingsService.UpdateAsync(apiResponse.Data.Id, apiResponse.Data);

            if (apiResponse.Success == false)
            {
                _messageBoxService.ShowErrorMessageBox(apiResponse.Message);
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _authenticationService.Logout();
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
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
