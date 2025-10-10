using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.Services
{
    public class ApplicationTextService : BaseHttpService, IApplicationTextService
    {
        private readonly IMapper _mapper;

        public ApplicationTextService(IClient client, ITokenStorage tokenStorage, IMapper mapper) : base(client, tokenStorage)
        {
            this._mapper = mapper;
        }

        public Task<Response<string>> GenerateApplicationText(int jobApplicationId, string userId) =>
            TryExecuteAsync(async () =>
            {
                var command = new GenerateApplicationTextCommand() {UserId = userId, JobApplicationId = jobApplicationId};
                var generatedTextDto = await _client.GenerateApplicationTextAsync(command);
                return generatedTextDto.ApplicationText;
            });
    }
}
