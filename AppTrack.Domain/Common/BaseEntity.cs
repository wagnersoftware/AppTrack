namespace AppTrack.Domain.Common;
public class BaseEntity
{
    public int Id { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

