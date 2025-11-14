using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.Cache;
using AppTrack.WpfUi.Contracts;
using AppTrack.WpfUi.CredentialManagement;
using AppTrack.WpfUi.Helpers;
using AppTrack.WpfUi.MessageBoxService;
using AppTrack.WpfUi.TokenStorage;
using AppTrack.WpfUi.ViewModel;
using AppTrack.WpfUi.WindowService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppTrack.Wpf;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    public static IConfiguration Configuration { get; private set; } = default!;

    public App()
    {
        SetEnvironmentVariables();

        string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();

        var services = new ServiceCollection();

        //api services
        services.AddApiServiceServices(Configuration);

        //window services
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();

        //validator
        services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));

        //helper, cache
        services.AddSingleton<ICredentialManager, CredentialManager>();
        services.AddSingleton<IUserHelper, UserHelper>();
        services.AddSingleton<IChatModelStore, ChatModelStore>();
        services.AddSingleton<ITokenStorage, WpfTokenStorage>();

        // viewmodels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<CreateJobApplicationViewModel>();
        services.AddTransient<EditJobApplicationViewModel>();
        services.AddTransient<SetJobApplicationDefaultsViewModel>();
        services.AddTransient<SetAiSettingsViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<EditKeyValueItemViewModel>();
        services.AddTransient<TextViewModel>();
        services.AddTransient<ApplicationTextViewModel>();
        services.AddTransient<GeneratedPromptViewModel>();

        //views
        services.AddTransient<MainWindow>();

        //models
        services.AddTransient<JobApplicationModel>();
        services.AddTransient<LoginModel>();
        services.AddTransient<RegistrationModel>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private static void SetEnvironmentVariables()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#else
    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
#endif
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var chatModelStore = ServiceProvider.GetRequiredService<IChatModelStore>();
        await chatModelStore.Initialize();

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
