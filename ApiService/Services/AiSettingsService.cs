using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.Services;

public class AiSettingsService : BaseHttpService, IAiSettingsService
{
    private readonly IMapper _mapper;

    public AiSettingsService(IMapper mapper, IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
        this._mapper = mapper;
    }

    public Task<Response<AiSettingsModel>> GetForUserAsync(string userId) =>
        TryExecuteAsync(async () =>
        {
            var aiSettingsDto = await _client.GetAiSettingsForUserAsync(userId);
            return _mapper.Map<AiSettingsModel>(aiSettingsDto);
        });

    public Task<Response<AiSettingsModel>> UpdateAsync(int id, AiSettingsModel aiSettingsModel) =>
        TryExecuteAsync<AiSettingsModel>(async () =>
        {
            var updateAiSettingsCommand = _mapper.Map<UpdateAiSettingsCommand>(aiSettingsModel);
            await _client.UpdateAiSettingsAsync(id, updateAiSettingsCommand);
        });
}

