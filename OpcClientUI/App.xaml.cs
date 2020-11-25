using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OpcClientUI.Services;
using OpcClientUI.ViewModels;
using OpcClientUI.Views;
using OpcClientUI.Models;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows;

namespace OpcClientUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {            
            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context.Configuration, services);
                    ServiceProvider = services.BuildServiceProvider();

                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true);                    
                })                
                .Build();            
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
            services.AddScoped<ISampleService, SampleService>();

            services.AddScoped<NavigationService>(serviceProvider =>
            {
                var navigationService = new NavigationService(serviceProvider);
                navigationService.Configure(OpcClientUI.Windows.MainWindow, typeof(MainWindow));
                navigationService.Configure(OpcClientUI.Windows.DetailWindow, typeof(DetailWindow));
                navigationService.Configure(OpcClientUI.Windows.WindowTwo, typeof(WindowTwo));
                return navigationService;
            });

            // Register all ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DetailViewModel>();
            services.AddSingleton<WindowTwoViewModel>();

            // Register all Views
            services.AddTransient<MainWindow>();
            services.AddTransient<DetailWindow>();
            services.AddTransient<WindowTwo>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();
            var navigationService = ServiceProvider.GetRequiredService<NavigationService>();

            await navigationService.ShowAsync(OpcClientUI.Windows.MainWindow);

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
    }
    public static class Windows
    {
        public const string MainWindow = nameof(MainWindow);
        public const string DetailWindow = nameof(DetailWindow);
        public const string WindowTwo = nameof(WindowTwo);
    }
}
