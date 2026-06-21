
using MediatR;
using HRManagement.Application.Features.Leave.Commands;

namespace HRManagement.Application.Services
{
    public class IncrementLeaveBalanceService : BackgroundService
    {
        private readonly  IServiceProvider _serviceProvider;
        private readonly ILogger<IncrementLeaveBalanceService> _logger;


        public IncrementLeaveBalanceService(IServiceProvider serviceProvider, ILogger<IncrementLeaveBalanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Leave Balance Service is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                //hitung next run untuk tanggal 1 bulan depan
                var nextRun = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                //var nextRun = now.AddSeconds(10);
                var duration = nextRun - now;

                await Task.Delay(duration, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(
                new UpdateLeaveBalanceCommand(),
                stoppingToken);


            }
        }
    }
}
