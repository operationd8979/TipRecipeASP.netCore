namespace TipRecipe.Filters
{
    public class MutithreadMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MutithreadMiddleware> _logger;
        private readonly AddInfoServerMiddleware _addInfoServerMiddleware;

        public MutithreadMiddleware(RequestDelegate next, ILogger<MutithreadMiddleware> logger, AddInfoServerMiddleware addInfoServerMiddleware)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _addInfoServerMiddleware = addInfoServerMiddleware ?? throw new ArgumentNullException(nameof(addInfoServerMiddleware));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            
            if (context.Request.Cookies.Count() > 0)
            {
                context.Request.Cookies.TryGetValue("jwt", out string? jwt);
                _logger.LogInformation($"MutithreadMiddleware: JWT token: {jwt}");
            }
            _logger.LogInformation("CombinedMiddleware: Starting parallel tasks");

            var task1 = _next(context);
            var task2 = _addInfoServerMiddleware.InvokeAsync(context);

            await Task.WhenAll(task1, task2);

            _logger.LogInformation("CombinedMiddleware: Completed parallel tasks");

        }


    }

    
}
