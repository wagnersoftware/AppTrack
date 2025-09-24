using AppTrack.WpfUi.View;
using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.WindowService;

public class WindowService : IWindowService
{
    public bool? ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        var owner = Application.Current.Windows
        .OfType<Window>()
        .FirstOrDefault(w => w.IsActive);

        Window window = viewModel switch
        {
            CreateJobApplicationViewModel vm => new CreateJobApplicationView(vm),
            EditJobApplicationViewModel vm => new EditJobApplicationView(vm),
            SetJobApplicationDefaultsViewModel vm => new SetJobApplicationDefaultsView(vm),
            SetAiSettingsViewModel vm => new SetAiSettingsView(vm),
            _ => throw new NotImplementedException()
        };

        if (owner is not null)
        {
            window.Owner = owner;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        return window.ShowDialog();
    }
}
