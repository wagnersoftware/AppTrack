namespace AppTrack.Application.Contracts.Mediator;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
