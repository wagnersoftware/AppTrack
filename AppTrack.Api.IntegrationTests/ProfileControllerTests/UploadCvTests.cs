using AppTrack.Api.IntegrationTests.WebApplicationFactory;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProfileControllerTests;

public class UploadCvTests : IClassFixture<FakeCvStorageWebApplicationFactory>
{
    private readonly HttpClient _client;

    // Minimal valid PDF header bytes — IPdfTextExtractor is stubbed, so content is irrelevant.
    private static readonly byte[] FakePdfBytes = "%PDF-1.4"u8.ToArray();

    public UploadCvTests(FakeCvStorageWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task UploadCv_ShouldReturn200_WhenFileIsValidPdf()
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(FakePdfBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        content.Add(fileContent, "file", "cv.pdf");

        var response = await _client.PostAsync("/api/profile/cv", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FreelancerProfileDto>();
        result.ShouldNotBeNull();
        result.CvBlobPath.ShouldBe(StubCvStorageService.FakeBlobPath);
        result.CvFileName.ShouldBe("cv.pdf");
        result.CvText.ShouldBe(StubPdfTextExtractor.FakeExtractedText);
        result.CvUploadDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task UploadCv_ShouldReturn400_WhenFileIsNotPdf()
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        content.Add(fileContent, "file", "document.txt");

        var response = await _client.PostAsync("/api/profile/cv", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
