using AppTrack.BlazorUI.Contracts;
using AppTrack.BlazorUI.Models.JobApplications;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUI.Pages.JobApplications
{
    public partial class Index
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJobApplicationService JobApplicationService { get; set; }

        public List<JobApplicationVM> JobApplications { get; private set; }

        public string Message { get; set; }

        protected void CreateJobApplication()
        {
            NavigationManager.NavigateTo("/jobapplications/create/");
        }

        protected void AllocateJobApplication(int id)
        {
            
        }

        protected void EditJobApplication(int id)
        {
            NavigationManager.NavigateTo($"/jobapplications/edit/{id}");
        }

        protected async Task DeleteJobApplication(int id)
        {
            var response = await JobApplicationService.DeleteJobApplication(id);

            if (response.Success)
            {
                StateHasChanged();
            }
            else
            {
                Message = response.Message;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            JobApplications = await JobApplicationService.GetJobApplicationsAsync();
        }
    }
}