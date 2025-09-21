using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models
{
    public class ModelBase
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
    }
}