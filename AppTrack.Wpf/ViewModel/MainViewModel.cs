using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.WpfUi.MessageBoxService;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel
{
    public partial class MainViewModel: ObservableObject
    {
        private readonly IJobApplicationService _jobApplicationService;
        private readonly IJobApplicationDefaultsService _jobApplicationDefaultsService;
        private readonly IWindowService _windowService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAiSettingsService _aiSettingsService;
        private readonly IApplicationTextService _applicationTextService;
        private readonly IMessageBoxService _messageBoxService;

        public ObservableCollection<JobApplicationModel> JobApplications { get; set; } = new();

        public MainViewModel(IJobApplicationService jobApplicationService,
                             IJobApplicationDefaultsService jobApplicationDefaultsService,
                             IWindowService windowService,
                             IServiceProvider serviceProvider,
                             IAiSettingsService aiSettingsService,
                             IApplicationTextService applicationTextService,
                             IMessageBoxService messageBoxService)
        {
            this._jobApplicationService = jobApplicationService;
            this._jobApplicationDefaultsService = jobApplicationDefaultsService;
            this._windowService = windowService;
            this._serviceProvider = serviceProvider;
            this._aiSettingsService = aiSettingsService;
            this._applicationTextService = applicationTextService;
            this._messageBoxService = messageBoxService;
        }

        public async Task LoadJobApplicationsAsync()
        {
            var apiResponse = await _jobApplicationService.GetJobApplicationsAsync();

            apiResponse.Data.ForEach(x => JobApplications.Add(x));
        }

        [RelayCommand]
        private async Task CreateJobApplication()
        {
            var response = await _jobApplicationDefaultsService.GetForUserAsync(1);// todo user

            var createJobApplicationViewModel = _serviceProvider.GetRequiredService<CreateJobApplicationViewModel>();
            createJobApplicationViewModel.SetDefaults(response.Data);

            var windowResult = _windowService.ShowWindow(createJobApplicationViewModel);

            if(windowResult == false)
            {
                return;
            }

            var apiResponse = await _jobApplicationService.CreateJobApplicationAsync(createJobApplicationViewModel.Model);

            if(apiResponse.Success == false)
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

            if(windowResult == false)
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
            var apiResponse = await _applicationTextService.GenerateApplicationText(jobApplicationModel.Id, 1, jobApplicationModel.URL, jobApplicationModel.Position); // todo UserId

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
            var apiResponse = await _jobApplicationDefaultsService.GetForUserAsync(1);// todo user

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
                return;
            }
        }

        [RelayCommand]
        private async Task AiSettings()
        {
            var apiResponse = await _aiSettingsService.GetForUserAsync(1); // todo user

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
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
