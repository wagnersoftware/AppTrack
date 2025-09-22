using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel
{
    public partial class MainViewModel: ObservableObject
    {
        private readonly IJobApplicationService _jobApplicationService;
        private readonly IWindowService _windowService;

        public ObservableCollection<JobApplicationModel> JobApplications { get; set; } = new();

        public MainViewModel(IJobApplicationService jobApplicationService, IWindowService windowService)
        {
            this._jobApplicationService = jobApplicationService;
            this._windowService = windowService;
        }

        public async Task LoadJobApplicationsAsync()
        {
            var jobApplications = await _jobApplicationService.GetJobApplicationsAsync();

            jobApplications.ForEach(x => JobApplications.Add(x));
        }

        [RelayCommand]
        private async Task CreateJobApplication()
        {
            var createJobApplicationViewModel = new CreateJobApplicationViewModel();
            var windowResult = _windowService.ShowWindow(createJobApplicationViewModel);

            if(windowResult == false)
            {
                return;
            }

            var apiResponse = await _jobApplicationService.CreateJobApplication(createJobApplicationViewModel.JobApplication);

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
    }
}
