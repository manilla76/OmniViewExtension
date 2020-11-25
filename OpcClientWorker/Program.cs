using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpcDatapoolLibrary;

namespace OpcClientWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())                
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    IConfiguration config = hostContext.Configuration;
                    ClientOptions options = config.GetSection("Client").Get<ClientOptions>();
                    services.AddSingleton(options);
                    services.AddTransient<DatapoolService>();
                    services.AddTransient<IClient, Client>();
                    
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterType<Client>().As<IClient>();
                    builder.RegisterType<Application>().As<IApplication>();                   
                });
    }
}
