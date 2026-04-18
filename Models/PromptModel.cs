using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTrack.Frontend.Models;

public partial class PromptModel : ModelBase, IPromptValidatable
{
    [ObservableProperty]
    private string key = string.Empty;

    [ObservableProperty]
    private string promptTemplate = string.Empty;

    public IEnumerable<PromptModel>? SiblingPrompts { get; set; }

    public Guid TempId { get; set; } = Guid.NewGuid();

    public PromptModel Clone()
    {
        return new PromptModel
        {
            Id = Id,
            Key = Key,
            PromptTemplate = PromptTemplate,
            CreationDate = CreationDate,
            ModifiedDate = ModifiedDate,
            SiblingPrompts = SiblingPrompts,
            TempId = TempId
        };
    }
}
