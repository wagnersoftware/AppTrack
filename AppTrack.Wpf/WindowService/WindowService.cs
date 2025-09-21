using AppTrack.WpfUi.View;
using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.WindowService;

public class WindowService : IWindowService
{
    public bool? ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        Window window = viewModel switch
        {
            CreateJobApplicationViewModel vm => new CreateJobApplicationView(vm),
            _ => throw new NotImplementedException()
        };

        return window.ShowDialog();
    }
}
