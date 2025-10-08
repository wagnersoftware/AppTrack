using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class EditKeyValueItemViewModel : AppTrackFormViewModelBase<PromptParameterModel>
{


    /// <summary>
    /// Constructor for editing existing item.
    /// </summary>
    /// <param name="modelValidator"></param>
    /// <param name="model"></param>
    public EditKeyValueItemViewModel(IModelValidator<PromptParameterModel> modelValidator, PromptParameterModel model) : base(modelValidator, model)
    {

    }

    /// <summary>
    /// Constructor for creating new item.
    /// </summary>
    /// <param name="modelValidator"></param>
    public EditKeyValueItemViewModel(IModelValidator<PromptParameterModel> modelValidator) : base(modelValidator, new PromptParameterModel())
    {

    }
}
