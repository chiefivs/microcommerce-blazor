using ActivityLogger.Service.Controllers;

namespace ActivityLogger.Service
{
    public class Scheduler : BackgroundService
    {
        private IServiceProvider ServiceProvider;

        public Scheduler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Timer timer = new Timer(new TimerCallback(PollEvents), stoppingToken, 2000, 2000);
            return Task.CompletedTask;
        }

        private async void PollEvents(object state)
        {
            try
            {
                //var logger = ServiceProvider.GetService<ActivityLoggerController>();
                //var task = logger.ReceiveEvents();
                //task.Wait();
            }
            catch
            {

            }
        }
    }
}
