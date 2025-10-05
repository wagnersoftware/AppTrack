using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.Contracts;
using AppTrack.WpfUi.CredentialManagement;
using AppTrack.WpfUi.Helpers;
using AppTrack.WpfUi.MessageBoxService;
using AppTrack.WpfUi.TokenStorage;
using AppTrack.WpfUi.ViewModel;
using AppTrack.WpfUi.WindowService;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppTrack.Wpf;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    public App()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITokenStorage, WpfTokenStorage>();
        services.AddApiServiceServices();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));
        services.AddSingleton<ICredentialManager, CredentialManager>();
        services.AddSingleton<IUserHelper, UserHelper>();

        // viewmodels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<CreateJobApplicationViewModel>();
        services.AddTransient<EditJobApplicationViewModel>();
        services.AddTransient<SetJobApplicationDefaultsViewModel>();
        services.AddTransient<SetAiSettingsViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<LoginViewModel>();

        //views
        services.AddTransient<MainWindow>();

        //models
        services.AddTransient<JobApplicationModel>();
        services.AddTransient<LoginModel>();
        services.AddTransient<RegistrationModel>();

        ServiceProvider = services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        base.OnStartup(e);

        var windowService = ServiceProvider.GetRequiredService<IWindowService>();
        var loginViewModel = ServiceProvider.GetRequiredService<LoginViewModel>();
        var isLoginSuccessful = windowService.ShowWindow(loginViewModel);

        if (isLoginSuccessful == true)
        {
            var viewModel = (MainViewModel)mainWindow.DataContext;
            await viewModel.LoadJobApplicationsForUserAsync();
        }
        else
        {
            Application.Current.Shutdown();
        }

    }
}
