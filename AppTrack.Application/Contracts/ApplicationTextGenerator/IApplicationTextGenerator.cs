﻿namespace AppTrack.Application.Contracts.ApplicationTextGenerator;

public interface IApplicationTextGenerator
{
    Task<string> GenerateApplicationTextAsync(string prompt, CancellationToken cancellationToken = default);
    void SetApiKey(string apiKey);
}
