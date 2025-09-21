using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.WpfUi.TokenStorage;
using AppTrack.WpfUi.View;
using AppTrack.WpfUi.ViewModel;
using AppTrack.WpfUi.WindowService;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppTrack.Wpf
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = default!;

        public App()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITokenStorage, WpfTokenStorage>();
            services.AddApiServiceServices();
            services.AddSingleton<IWindowService, WindowService>();

            // viewmodels
            services.AddSingleton<MainViewModel>();
            //services.AddTransient<CreateJobApplicationViewModel>();

            //views
            services.AddTransient<MainWindow>();
            //services.AddTransient<CreateJobApplicationView>();

            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}
