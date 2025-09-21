using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

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

            if(windowResult == true)
            {
                var apiResponse = await _jobApplicationService.CreateJobApplication(createJobApplicationViewModel.JobApplication);
                JobApplications.Add(apiResponse.Data);
            }
        }
    }
}
