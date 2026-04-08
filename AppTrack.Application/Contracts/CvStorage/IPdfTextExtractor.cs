namespace AppTrack.Application.Contracts.CvStorage;

public interface IPdfTextExtractor
{
    /// <summary>
    /// Extracts all text from the PDF stream.
    /// The caller is responsible for resetting stream position after this call if reuse is needed.
    /// </summary>
    string ExtractText(Stream stream);
}
