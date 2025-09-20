using AppTrack.Frontend.ApiService;
using AppTrack.WpfUi.ViewModel;
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
            services.AddApiServiceServices();

            // viewmodels
            services.AddSingleton<MainViewModel>();

            //views
            services.AddTransient<MainWindow>();

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
