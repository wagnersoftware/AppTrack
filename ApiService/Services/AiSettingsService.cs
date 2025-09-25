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

    public async Task<AiSettingsModel> GetForUserAsync(int userId)
    {
        await AddBearerTokenAsync();
        var aiSettingsDto = await _client.GetAiSettingsForUserAsync(userId);
        return _mapper.Map<AiSettingsModel>(aiSettingsDto);
    }

    public async Task UpdateAsync(int id, AiSettingsModel aiSettingsModel)
    {
        await AddBearerTokenAsync();
        var updateAiSettingsCommand = _mapper.Map<UpdateAiSettingsCommand>(aiSettingsModel);
        await _client.UpdateAiSettingsAsync(id, updateAiSettingsCommand);
    }
}

