namespace AppTrack.Domain.Extensions;

public static class JobApplicationExtensions
{
    public static List<PromptParameter> ToPromptParameters(this JobApplication jobApplication)
    {
        return new List<PromptParameter>
        {
            PromptParameter.Create(nameof(jobApplication.Location), jobApplication.Location),
            PromptParameter.Create(nameof(jobApplication.ContactPerson), jobApplication.ContactPerson),
            PromptParameter.Create(nameof(jobApplication.Position), jobApplication.Position),
            PromptParameter.Create(nameof(jobApplication.DurationInMonths), jobApplication.DurationInMonths?.ToString()),
            PromptParameter.Create(nameof(jobApplication.JobDescription), jobApplication.JobDescription),
            PromptParameter.Create(nameof(jobApplication.URL), jobApplication.URL),
            PromptParameter.Create(nameof(jobApplication.StartDate), jobApplication.StartDate.ToString("yyyy-MM-dd"))
        };
    }
}
