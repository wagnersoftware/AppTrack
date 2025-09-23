using AppTrack.Application.Contracts.Mediator;

namespace AppTrack.Infrastructure.Mediator;

public class Mediator : IMediator
{
    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        var handler = _provider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler for type {request.GetType().Name} found");
        }
            

        return (Task<TResponse>)handlerType
            .GetMethod("Handle")!
            .Invoke(handler, new object[] { request, cancellationToken })!;
    }
}
