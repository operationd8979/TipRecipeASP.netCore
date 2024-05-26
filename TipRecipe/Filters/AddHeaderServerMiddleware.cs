namespace TipRecipe.Filters
{
    public class AddHeaderServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AddHeaderServerMiddleware> _logger;

        public AddHeaderServerMiddleware(RequestDelegate next, ILogger<AddHeaderServerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Middleware1: Before next delegate");

            await _next(context);

            _logger.LogInformation("Middleware1: After next delegate");
        }
    }
}
