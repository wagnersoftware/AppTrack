using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTrack.Frontend.Models;

public partial class PromptParameterModel : ModelBase, IPromptParameterValidatable
{
    [ObservableProperty]
    private string key = string.Empty;

    [ObservableProperty]
    private string value = string.Empty;

    public IEnumerable<PromptParameterModel>? ParentCollection { get; set; }

    public Guid TempId { get; set; } = Guid.NewGuid();

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
