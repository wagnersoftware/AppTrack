using AppTrack.Application.Contracts.CvStorage;
using System.Text;
using UglyToad.PdfPig;

namespace AppTrack.Infrastructure.CvStorage;

public class PdfPigTextExtractor : IPdfTextExtractor
{
    public string ExtractText(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var sb = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            sb.Append(page.Text);
        }
        return sb.ToString();
    }
}
