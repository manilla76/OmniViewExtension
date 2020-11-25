using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace OpcClientWorker
{
    public class Worker : BackgroundService
    {
        private readonly IApplication _application;
        // private readonly IServiceProvider _services;
        //private readonly ILogger _logger;
        
        public Worker(IApplication application)
        {            
            _application = application;
            //_services = services;
            //_logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // do some stuff that needs to be done while the worker is running.  
            }
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            //clients.Add(_services.GetRequiredService<IClient>());

            // Initialize the client   
            //_logger.LogInformation($"Client Startup at {DateTime.Now}");
            await _application.AddClientAsync();
            //_logger.LogInformation($"Client Added"); 
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _application.RemoveClientAsync();   
        }



        
    }
}
