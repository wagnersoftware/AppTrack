using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models.Base
{
    public class ModelBase : ObservableValidator
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}