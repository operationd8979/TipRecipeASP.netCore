
using TipRecipe.DbContexts;

namespace TipRecipe.Services
{
    public class DishBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DishBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

        public DishBackgroundService(IServiceProvider serviceProvider, ILogger<DishBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dishService = scope.ServiceProvider.GetRequiredService<DishService>();
                        await dishService.UpdateAverageScoreDishes();
                    }
                    _logger.LogInformation("Updating average scores...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating average scores.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

    }
}
