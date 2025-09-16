using AppTrack.BlazorUI.Services.Base;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.BlazorUI.Models.JobApplications
{
    public class JobApplicationVM
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Company name")]
        public string Client { get; set; } = string.Empty;
        [Required]
        public string Position { get; set; } = string.Empty;
        public JobApplicationStatus Status { get; set; }
        public DateTime? AppliedDate { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ApplicationText { get; set; } = string.Empty;
    }
}
