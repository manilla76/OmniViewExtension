using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThermoPileModelLibrary;

namespace ThermoPileModelService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PileModel _pile;
        private readonly double tick;
        private readonly System.Timers.Timer timer;

        public Worker(ILogger<Worker> logger, ThermoPileModelLibrary.PileModel pile)
        {
            _logger = logger;
            _pile = pile;
            tick = 1000f;
            timer = new System.Timers.Timer(tick);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;

        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Advance Stacker
            _pile.MoveStacker(tick/1000);    // Move the stacker ahead 1 second
            //Add Material
            _pile.AddTons(tick / 1000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
