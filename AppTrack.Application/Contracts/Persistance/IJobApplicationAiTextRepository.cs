using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationAiTextRepository
{
    Task<int> CountByJobApplicationAndPromptAsync(int jobApplicationId, string promptName);
    Task<List<JobApplicationAiText>> GetOldestByJobApplicationAndPromptAsync(int jobApplicationId, string promptName, int keepNewest);
    Task AddAsync(JobApplicationAiText aiText);
    Task DeleteAsync(JobApplicationAiText aiText);
}
