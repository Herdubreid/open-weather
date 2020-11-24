using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenWeather
{
    public static class DistributedCacheExtensions
    {
        static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            Converters =
            {
                new TupleJsonConverter<decimal, decimal>(),
                new TupleJsonConverter<int, int>()
            }
        };
        static DistributedCacheEntryOptions CacheOptions { get; } = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(80)
        };
        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
            string recordId,
            T data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await cache.SetStringAsync(recordId, json);
        }
        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            var json = await cache.GetStringAsync(recordId);

            if (json is null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
    }
}
