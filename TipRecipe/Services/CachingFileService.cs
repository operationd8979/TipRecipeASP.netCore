using System.Text.Json;
using TipRecipe.Models;

namespace TipRecipe.Services
{
    public class CachingFileService
    {
        private readonly string _cacheFilePath;
        private readonly ILogger<CachingFileService> _logger;

        public CachingFileService(string cacheFilePath, ILogger<CachingFileService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheFilePath = cacheFilePath;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan duration)
        {
            var cacheData = await ReadCacheFileAsync();

            var cacheItem = new CacheItem(value!, DateTime.UtcNow.Add(duration));

            cacheData[key] = cacheItem;

            var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_cacheFilePath, json);
            _logger.LogInformation($"Wrote cache $[{key}] into ${_cacheFilePath}");
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cacheData = await ReadCacheFileAsync();
            if (cacheData.TryGetValue(key, out var cacheItem))
            {
                var cacheItemTyped = JsonSerializer.Deserialize<CacheItem>(cacheItem.ToString()!);
                if (cacheItemTyped != null && cacheItemTyped.Expiration > DateTime.UtcNow)
                {
                    _logger.LogInformation($"Read cache for key [{key}] from {_cacheFilePath}");
                    return JsonSerializer.Deserialize<T>(cacheItemTyped.Value!.ToString()!);
                }
                _logger.LogInformation($"Cache expired for key [{key}]");
                cacheData.Remove(key);
                var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_cacheFilePath, json);
            }
            return default;
        }

        private async Task<Dictionary<string, object>> ReadCacheFileAsync()
        {
            if (!File.Exists(_cacheFilePath))
            {
                return new Dictionary<string, object>();
            }

            var json = await File.ReadAllTextAsync(_cacheFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }
    }
}
