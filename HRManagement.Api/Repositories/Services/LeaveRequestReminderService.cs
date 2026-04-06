
using HRManagement.Api.Application.Commands.LeaveManagementCommands;
using MediatR;

namespace HRManagement.Api.Repositories.Services
{
    public class LeaveRequestReminderService: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LeaveRequestReminderService> _logger;


        public LeaveRequestReminderService(IServiceProvider serviceProvider, ILogger<LeaveRequestReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("LeaveRequestReminderService is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                //hitung next run untuk tanggal 1 bulan depan
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);

                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);
                var duration = nextRun - now;

                await Task.Delay(duration, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new UpdateLeaveBalanceCommand(), stoppingToken);

                _logger.LogInformation("Leave reminder job executed at {time}", DateTime.Now);
            }
        }
    }
}
