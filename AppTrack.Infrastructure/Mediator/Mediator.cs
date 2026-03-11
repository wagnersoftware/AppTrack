using AppTrack.Application.Contracts;
using AppTrack.Application.Contracts.Mediator;

namespace AppTrack.Infrastructure.Mediator;

/// <summary>
/// Custom mediator implementation that dispatches <see cref="IRequest{TResponse}"/> objects
/// to their registered <see cref="IRequestHandler{TRequest,TResponse}"/> handlers.
/// </summary>
/// <remarks>
/// Before each dispatch, if the request implements <see cref="IUserScopedRequest"/>, the
/// authenticated user's ID is resolved from <see cref="IUserContext"/> and set on the request.
/// This guarantees that <c>UserId</c> always originates from verified JWT claims and can never
/// be supplied or overridden by API callers.
/// </remarks>
public class Mediator : IMediator
{
    private readonly IServiceProvider _provider;
    private readonly IUserContext _userContext;

    public Mediator(IServiceProvider provider, IUserContext userContext)
    {
        _provider = provider;
        _userContext = userContext;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is IUserScopedRequest userScopedRequest)
        {
            if (!_userContext.IsAuthenticated)
                throw new InvalidOperationException(
                    $"Request '{request.GetType().Name}' implements IUserScopedRequest but was dispatched without an authenticated user.");

            userScopedRequest.UserId = _userContext.GetCurrentUserId();
        }

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
