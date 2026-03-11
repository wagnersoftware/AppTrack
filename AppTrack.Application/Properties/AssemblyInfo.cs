using System.Runtime.CompilerServices;

// Allow AppTrack.Infrastructure to call internal members of AppTrack.Application.
// This is used exclusively by UserScopedRequestHandlerDecorator to call
// IUserScopedRequest.SetUserId(), keeping the setter inaccessible to all other assemblies
// including AppTrack.Api (the controller layer).
[assembly: InternalsVisibleTo("AppTrack.Infrastructure")]

// Allow unit test project to construct commands/queries with a UserId for testing purposes.
[assembly: InternalsVisibleTo("AppTrack.Application.UnitTests")]
