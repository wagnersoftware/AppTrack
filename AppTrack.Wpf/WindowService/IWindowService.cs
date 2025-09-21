namespace AppTrack.WpfUi.WindowService;

public interface IWindowService
{
    bool? ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : class;
}
