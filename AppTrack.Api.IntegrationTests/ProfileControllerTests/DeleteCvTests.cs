using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.IntegrationTests.WebApplicationFactory;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProfileControllerTests;

/// <summary>
/// Profile has no CV — ICvStorageService is never called, plain factory is sufficient.
/// </summary>
public class DeleteCvNoCvTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteCvNoCvTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task DeleteCv_ShouldReturn200_WhenProfileHasNoCv()
    {
        await SeedHelper.CreateFreelancerProfileForTestUserAsync(_factory.Services);

        var response = await _client.DeleteAsync("/api/profile/cv");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FreelancerProfileDto>();
        result.ShouldNotBeNull();
        result.CvBlobPath.ShouldBeNull();
        result.CvFileName.ShouldBeNull();
    }
}

/// <summary>
/// Profile has a CV — DeleteAsync on ICvStorageService is called; uses stub factory.
/// </summary>
public class DeleteCvWithCvTests : IClassFixture<FakeCvStorageWebApplicationFactory>
{
    private readonly FakeCvStorageWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteCvWithCvTests(FakeCvStorageWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task DeleteCv_ShouldReturn200AndClearCvFields_WhenProfileHasCv()
    {
        await SeedHelper.CreateFreelancerProfileWithCvForTestUserAsync(_factory.Services);

        var response = await _client.DeleteAsync("/api/profile/cv");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FreelancerProfileDto>();
        result.ShouldNotBeNull();
        result.CvBlobPath.ShouldBeNull();
        result.CvFileName.ShouldBeNull();
        result.CvText.ShouldBeNull();
    }
}
