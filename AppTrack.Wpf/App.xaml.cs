using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
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

        // viewmodels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<CreateJobApplicationViewModel>();
        services.AddTransient<EditJobApplicationViewModel>();
        services.AddTransient<SetJobApplicationDefaultsViewModel>();
        services.AddTransient<SetAiSettingsViewModel>();

        //views
        services.AddTransient<MainWindow>();

        //models
        services.AddTransient<JobApplicationModel>();
        services.AddTransient<LoginModel>();

        ServiceProvider = services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        base.OnStartup(e);

        var windowService = ServiceProvider.GetRequiredService<IWindowService>();
        var loginViewModel = ActivatorUtilities.CreateInstance<LoginViewModel>(ServiceProvider);
        var isLoginSuccessful = windowService.ShowWindow(loginViewModel);

        if(isLoginSuccessful == true)
        {
            var viewModel = (MainViewModel)mainWindow.DataContext;
            await viewModel.LoadJobApplicationsAsync();
        }
        else
        {
            Application.Current.Shutdown();
        }

    }
}
