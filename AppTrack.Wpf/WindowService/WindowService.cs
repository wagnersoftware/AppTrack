using AppTrack.WpfUi.View;
using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.WindowService;

public class WindowService : IWindowService
{
    public bool? ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        var owner = GetActiveWindow();

        Window window = viewModel switch
        {
            CreateJobApplicationViewModel vm => new JobApplicationView(vm),
            EditJobApplicationViewModel vm => new JobApplicationView(vm),
            SetJobApplicationDefaultsViewModel vm => new SetJobApplicationDefaultsView(vm),
            SetAiSettingsViewModel vm => new SetAiSettingsView(vm),
            LoginViewModel vm => new LoginView(vm),
            RegistrationViewModel vm => new RegistrationView(vm),
            EditKeyValueItemViewModel vm => new EditKeyValueItemView(vm),
            GeneratedPromptViewModel vm => new GeneratedPromptView(vm),
            ApplicationTextViewModel vm => new TextView(vm),
            _ => throw new NotImplementedException()
        };

        if (owner is not null)
        {
            window.Owner = owner;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        return window.ShowDialog();
    }

    private static Window GetActiveWindow() =>
    Application.Current.Windows
        .OfType<Window>()
        .FirstOrDefault(w => w.IsActive)
    ?? Application.Current.MainWindow;
}
