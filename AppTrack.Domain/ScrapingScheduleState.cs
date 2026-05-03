namespace AppTrack.Domain;

public class ScrapingScheduleState
{
    public int Id { get; set; }
    public DateTime NextRunAfterUtc { get; set; }
}
