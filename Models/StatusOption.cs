namespace AppTrack.Frontend.Models;

public class StatusOption
{
    public JobApplicationModel.JobApplicationStatus? Status { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
