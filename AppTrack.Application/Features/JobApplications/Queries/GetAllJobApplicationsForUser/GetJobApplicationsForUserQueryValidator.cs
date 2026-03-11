using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;

public class GetJobApplicationsForUserQueryValidator : AbstractValidator<GetJobApplicationsForUserQuery>
{
    public GetJobApplicationsForUserQueryValidator()
    {
    }
}
