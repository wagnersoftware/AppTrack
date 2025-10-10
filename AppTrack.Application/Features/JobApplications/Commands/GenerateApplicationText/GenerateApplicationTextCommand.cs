using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommand : IRequest<GeneratedApplicationTextDto>
{
    public int JobApplicationId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
