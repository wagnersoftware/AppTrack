using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
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

        public ObservableCollection<JobApplicationModel> JobApplications { get; set; } = new();

        public MainViewModel(IJobApplicationService jobApplicationService,
                             IJobApplicationDefaultsService jobApplicationDefaultsService,
                             IWindowService windowService,
                             IServiceProvider serviceProvider,
                             IAiSettingsService aiSettingsService)
        {
            this._jobApplicationService = jobApplicationService;
            this._jobApplicationDefaultsService = jobApplicationDefaultsService;
            this._windowService = windowService;
            this._serviceProvider = serviceProvider;
            this._aiSettingsService = aiSettingsService;
        }

        public async Task LoadJobApplicationsAsync()
        {
            var jobApplications = await _jobApplicationService.GetJobApplicationsAsync();

            jobApplications.ForEach(x => JobApplications.Add(x));
        }

        [RelayCommand]
        private async Task CreateJobApplication()
        {
            var jobApplicationDefaults = await _jobApplicationDefaultsService.GetForUserAsync(1);// todo user

            var createJobApplicationViewModel = _serviceProvider.GetRequiredService<CreateJobApplicationViewModel>();
            createJobApplicationViewModel.SetDefaults(jobApplicationDefaults);

            var windowResult = _windowService.ShowWindow(createJobApplicationViewModel);

            if(windowResult == false)
            {
                return;
            }

            var apiResponse = await _jobApplicationService.CreateJobApplication(createJobApplicationViewModel.Model);

            if(apiResponse.Success == false)
            {
                MessageBox.Show(apiResponse.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); //MessageBoxService
            }

            JobApplications.Add(apiResponse.Data);
            
        }

        [RelayCommand]
        private async Task DeleteJobApplication(int id)
        {
            var dialogResult = MessageBox.Show($"Do you really want to delete application with id: {id}", "Delete application", MessageBoxButton.OKCancel, MessageBoxImage.Question); //MessageBoxService

            if (dialogResult == MessageBoxResult.No || dialogResult == MessageBoxResult.Cancel)
            {
                return;
            }

            var apiResponse = await _jobApplicationService.DeleteJobApplication(id);

            if (apiResponse.Success == false)
            {
                MessageBox.Show(apiResponse.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); //MessageBoxService
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

            var apiResponse = await _jobApplicationService.UpdateJobApplication(jobApplicationModel.Id, jobApplicationModel);

            if (apiResponse.Success == false)
            {
                MessageBox.Show(apiResponse.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); //MessageBoxService
            }

        }

        [RelayCommand]
        private async Task SetDefaults()
        {
            var jobApplicationDefaults = await _jobApplicationDefaultsService.GetForUserAsync(1);// todo user

            var jobApplicatiobDefaultsViewModel = ActivatorUtilities.CreateInstance<SetJobApplicationDefaultsViewModel>(_serviceProvider, jobApplicationDefaults);
            var windowResult = _windowService.ShowWindow(jobApplicatiobDefaultsViewModel);

            if (windowResult == false)
            {
                return;
            }

            await _jobApplicationDefaultsService.UpdateAsync(jobApplicationDefaults.Id, jobApplicationDefaults);
        }

        [RelayCommand]
        private async Task AiSettings()
        {
            var aiSettingsModel = await _aiSettingsService.GetForUserAsync(1); // todo user
            var setAiSettingsViewModel = ActivatorUtilities.CreateInstance<SetAiSettingsViewModel>(_serviceProvider, aiSettingsModel);
            var windowResult = _windowService.ShowWindow(setAiSettingsViewModel);

            if (windowResult == false)
            {
                return;
            }

            await _aiSettingsService.UpdateAsync(aiSettingsModel.Id, aiSettingsModel);
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
