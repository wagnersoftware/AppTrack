using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationAiTextRepository
{
    Task<int> CountByJobApplicationAndPromptAsync(int jobApplicationId, string promptKey);
    Task<JobApplicationAiText?> GetOldestByJobApplicationAndPromptAsync(int jobApplicationId, string promptKey);
    Task AddAsync(JobApplicationAiText aiText);
    Task DeleteAsync(JobApplicationAiText aiText);
}
