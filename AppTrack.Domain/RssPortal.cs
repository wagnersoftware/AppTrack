using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;

public class RssPortal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public RssParserType ParserType { get; set; } = RssParserType.Default;
    public bool IsActive { get; set; } = true;
}
