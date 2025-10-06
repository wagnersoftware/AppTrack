using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class EditKeyValueItemViewModel : AppTrackFormViewModelBase<KeyValueItemModel>
{
    /// <summary>
    /// Constructor for editing existing item.
    /// </summary>
    /// <param name="modelValidator"></param>
    /// <param name="model"></param>
    public EditKeyValueItemViewModel(IModelValidator<KeyValueItemModel> modelValidator, KeyValueItemModel model) : base(modelValidator, model)
    {
    }

    /// <summary>
    /// Constructor for creating new item.
    /// </summary>
    /// <param name="modelValidator"></param>
    public EditKeyValueItemViewModel(IModelValidator<KeyValueItemModel> modelValidator) : base(modelValidator, new KeyValueItemModel())
    {
    }
}
