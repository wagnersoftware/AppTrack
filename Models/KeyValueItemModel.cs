using AppTrack.Frontend.Models.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class KeyValueItemModel : ModelBase
{
    [Required]
    [ObservableProperty]
    private string key = string.Empty;

    [Required]
    [ObservableProperty]
    private string value = string.Empty;

    public KeyValueItemModel Clone()
    {
        return new KeyValueItemModel
        {
            Id = Id,
            Key = Key,
            Value = Value,
            CreationDate = CreationDate,
            ModifiedDate = ModifiedDate,
        };
    }
}
