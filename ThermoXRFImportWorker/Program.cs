using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.IO;
using System.Net.Sockets;

namespace ThermoXRFImportWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(LoggerFactory => 
                {
                    LoggerFactory.AddEventLog();                   
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.Add(new ServiceDescriptor(typeof(FileSystemWatcher), new FileSystemWatcher()));
                    services.Add(new ServiceDescriptor(typeof(TcpClient), new TcpClient()));
                });
    }
}
