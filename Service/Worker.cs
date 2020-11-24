using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenWeather;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Service
{
    public class Worker : BackgroundService
    {
        IConfiguration Config { get; }
        ILogger<Worker> Logger { get; }
        IDistributedCache Cache { get; }
        JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            Converters =
            {
                new DateJsonConverter(),
                new TimeSpanConverter()
            }
        };
        const string RedisKey = "Trend";
        static readonly int[] keys = new int[] { 3413829, 2618425, 1566083, 2063523 };
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var trend =
                await Cache.GetRecordAsync<Dictionary<int, List<Trend>>>(RedisKey)
                ?? new Dictionary<int, List<Trend>>();
            Logger.LogInformation("Loaded {0} existing rows.", trend.Count);
            string endpoint = $"http://api.openweathermap.org/data/2.5/group?id={string.Join(',', keys)}&units=metric&appid={Config["OpenWeatherApiKey"]}";
            while (!stoppingToken.IsCancellationRequested)
            {
                var waitMins = (85 - DateTime.Now.Minute) % 60;
                Logger.LogInformation("{0}, waiting until {1}, {2} minutes...", DateTimeOffset.Now, DateTime.Now.AddMinutes(waitMins), waitMins);
                await Task.Delay((int) TimeSpan.FromMinutes(waitMins).TotalMilliseconds, stoppingToken);
                Logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    using (var http = new HttpClient())
                    {
                        var response = await http.GetAsync(endpoint);
                        var temp = JsonSerializer.Deserialize<Response>(
                            await response.Content.ReadAsStringAsync(), JsonOptions);
                        foreach (var m in temp.list)
                        {
                            if (trend.TryGetValue(m.id, out var t))
                            {
                                var last = t[t.Count - 1];
                                if (!last.TimeStamp.Equals(m.dt))
                                {
                                    t.Add(Trend.Add(t[t.Count - 1], m));
                                }
                                if (t.Count > 48)
                                {
                                    t.RemoveAt(0);
                                }
                            }
                            else
                            {
                                trend.Add(m.id, new List<Trend>(new Trend[] { Trend.Add(null, m) }));
                            }
                        }
                        await Cache.SetRecordAsync(RedisKey, trend);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                await Task.Delay((int)TimeSpan.FromMinutes(2).TotalMilliseconds, stoppingToken);
            }
        }
        public Worker(IConfiguration config, ILogger<Worker> logger, IDistributedCache cache)
        {
            Config = config;
            Logger = logger;
            Cache = cache;
        }
    }
}
