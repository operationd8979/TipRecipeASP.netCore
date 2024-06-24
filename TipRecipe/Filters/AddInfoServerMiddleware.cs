using Microsoft.AspNetCore.Http;

namespace TipRecipe.Filters
{
    public class AddInfoServerMiddleware
    {

        private readonly ILogger<AddInfoServerMiddleware> _logger;

        public AddInfoServerMiddleware(ILogger<AddInfoServerMiddleware> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("AddInfoServerMiddleware: Adding server name to response header");
            string serverName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "UnknownServer";
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Server-Name", serverName);
                return Task.CompletedTask;
            });
            for (int i = 0; i < 10; i++)
            {
                _logger.LogInformation("Task AddInfoServerMiddleware {I}",i);
                await Task.Delay(1000);
            }
            _logger.LogInformation("Task background completed");
        }
    }
}
