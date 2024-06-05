

namespace TipRecipe.Services
{
    public class DishBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DishBackgroundService> _logger;
        private readonly TimeSpan _intervalUpdateAvgScoreDishes = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _intervalUpdateSASTokenStorageBlob = TimeSpan.FromHours(1.5);

        public DishBackgroundService(IServiceProvider serviceProvider, ILogger<DishBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var task1 = TaskUpdateAverageScoreDishesAsync(stoppingToken);
            var task2 = TaskUpdateSASTokenStorageBlobAsync(stoppingToken);
            await Task.WhenAll(task1, task2);
        }

        private async Task TaskUpdateAverageScoreDishesAsync(CancellationToken stoppingToken)
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

                await Task.Delay(_intervalUpdateAvgScoreDishes, stoppingToken);
            }
        }

        private async Task TaskUpdateSASTokenStorageBlobAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var azureBlobService = scope.ServiceProvider.GetRequiredService<AzureBlobService>();
                        azureBlobService.UpdateSasTokensForContainers();
                    }
                    _logger.LogInformation("Updating average scores...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating average scores.");
                }

                await Task.Delay(_intervalUpdateSASTokenStorageBlob, stoppingToken);
            }
        }

    }
}
