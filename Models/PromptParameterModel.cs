using AppTrack.Frontend.Models.Base;
using AppTrack.Frontend.Models.ValidationAttributes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class PromptParameterModel : ModelBase
{
    [Required]
    [ObservableProperty]
    [UniqueKey("ParentCollection", "TempId")]
    [MaxLength(50, ErrorMessage = "{0} must not exceed 50 characters.")]
    private string key = string.Empty;

    [Required]
    [ObservableProperty]
    private string value = string.Empty;

    public IEnumerable<PromptParameterModel>? ParentCollection { get; set; }// the current items, for key unique validation

    public Guid TempId { get; set; } = Guid.NewGuid(); // for comparing instances, if Id is not set yet by the database

    public PromptParameterModel Clone()
    {
        return new PromptParameterModel
        {
            Id = Id,
            Key = Key,
            Value = Value,
            CreationDate = CreationDate,
            ModifiedDate = ModifiedDate,
            ParentCollection = ParentCollection,
            TempId = TempId
        };
    }
}
