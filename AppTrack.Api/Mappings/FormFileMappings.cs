using AppTrack.Application.Features.FreelancerProfile.Commands.UploadCv;

namespace AppTrack.Api.Mappings;

public static class FormFileMappings
{
    public static UploadCvCommand ToUploadCvCommand(this IFormFile file) => new()
    {
        FileStream = file.OpenReadStream(),
        FileName = file.FileName,
        ContentType = file.ContentType,
        FileSizeBytes = file.Length,
    };
}
