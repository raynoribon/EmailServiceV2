using System.Diagnostics;

namespace EmailReceiverService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);                
                Receiver receiver = new Receiver();
                receiver.Process();
                Common.ReceiverLog("Sleep for 60 secs");
                await Task.Delay(60000, stoppingToken); //sleeping for 60 secs
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("WriterWorker started at: {time} and will take 5 seconds to complete.", DateTimeOffset.Now);
            Common.ReceiverLog("Email Receiver started");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var stopWatch = Stopwatch.StartNew();
            //_logger.LogInformation("WriterWorker stopped at: {time}", DateTimeOffset.Now);
            Common.ReceiverLog("Email Receiver  stopped");
            await base.StopAsync(cancellationToken);
            //_logger.LogInformation("WriterWorker took {ms} ms to stop.", stopWatch.ElapsedMilliseconds);
        }

    }
}