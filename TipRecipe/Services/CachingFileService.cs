﻿using Serilog;
using System.Text.Json;
using TipRecipe.Models;

namespace TipRecipe.Services
{
    public class CachingFileService
    {
        private readonly ILogger<CachingFileService> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly string _cacheFilePath = "Caches/cachefile.json";

        public CachingFileService(ILogger<CachingFileService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cacheData = await ReadCacheFileAsync();
            if (cacheData.TryGetValue(key, out var cacheItem))
            {
                var cacheItemTyped = JsonSerializer.Deserialize<CacheItem>(cacheItem.ToString()!);
                if (cacheItemTyped != null && cacheItemTyped.Expiration > DateTime.UtcNow)
                {
                    _logger.LogInformation("Read cache for key [{Key}] from {_cacheFilePath}",key,_cacheFilePath);
                    return JsonSerializer.Deserialize<T>(cacheItemTyped.Value!.ToString()!);
                }
                _logger.LogInformation("Cache expired for key [{Key}]",key);
                cacheData.Remove(key);
                await _semaphore.WaitAsync();
                try
                {
                    var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(_cacheFilePath, json);
                    _logger.LogInformation("Wrote cache $[{Key}] into ${_cacheFilePath}", key, _cacheFilePath);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return default;
        }
        

        public async Task SetAsync<T>(string key, T value, TimeSpan duration)
        {
            var cacheData = await ReadCacheFileAsync();

            var cacheItem = new CacheItem(value!, DateTime.UtcNow.Add(duration));

            cacheData[key] = cacheItem;

            await _semaphore.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_cacheFilePath, json);
                _logger.LogInformation("Wrote cache $[{Key}] into ${_cacheFilePath}",key,_cacheFilePath);
            }
            finally
            {
                _semaphore.Release();
            }

        }

        public async Task<bool> UpdateAsync<T>(string key, T value)
        {
            var cacheData = await ReadCacheFileAsync();
            if (cacheData.TryGetValue(key, out var cacheItem))
            {
                var cacheItemTyped = JsonSerializer.Deserialize<CacheItem>(cacheItem.ToString()!);
                if (cacheItemTyped != null)
                {
                    cacheItemTyped.Value = value;
                    cacheData[key] = cacheItemTyped;

                    await _semaphore.WaitAsync();
                    try
                    {
                        var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(_cacheFilePath, json);
                        _logger.LogInformation("Update cache $[{Key}] into ${_cacheFilePath}",key,_cacheFilePath);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                    return true;
                }
            }
            return false;
        }

        private async Task<Dictionary<string, object>> ReadCacheFileAsync()
        {
            await _semaphore.WaitAsync();
            Dictionary<string, object> result = new();
            try
            {
                if (!File.Exists(_cacheFilePath))
                {
                    throw new FileNotFoundException($"Cache file {_cacheFilePath} not found");
                }
                var json = await File.ReadAllTextAsync(_cacheFilePath);
                result = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            finally
            {
                _semaphore.Release();
            }
            return result;
        }

    }
}
