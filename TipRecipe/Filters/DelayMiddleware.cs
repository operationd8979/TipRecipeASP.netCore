namespace TipRecipe.Filters
{
    public class DelayMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<DelayMiddleware> _logger;

        public DelayMiddleware(RequestDelegate next, ILogger<DelayMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("DelayMiddleware: Starting delay task");

            for (int i = 0; i < 5; i++)
            {
                _logger.LogInformation($"Task DelayMiddleware {i}");
                await Task.Delay(1000);
            }
            await _next(context);

            _logger.LogInformation("DelayMiddleware: Completed delay task");

        }
    }
}
