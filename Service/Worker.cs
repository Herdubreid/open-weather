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
using System.Linq;

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
        const string RedisKeyM = "Trend";
        const string RedisKeyF = "Forecast";
        static readonly int[] keys = new int[] { 3413829, 2618425, 1566083, 2063523 };
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var trend =
                await Cache.GetRecordAsync<Dictionary<int, List<Trend>>>(RedisKeyM)
                ?? new Dictionary<int, List<Trend>>();
            Logger.LogInformation("Loaded {0} existing Trend rows.", trend.Count);
            var forecast =
                await Cache.GetCompressedRecordAsync<List<Data>>(RedisKeyF)
                ?? new List<Data>();
            Logger.LogInformation("Loaded {0} existing Forecast rows.", forecast.Count);
            string endpointM = $"http://api.openweathermap.org/data/2.5/group?id={string.Join(',', keys)}&units=metric&appid={Config["OpenWeatherApiKey"]}";
            while (!stoppingToken.IsCancellationRequested)
            {
                var waitMins = (85 - DateTime.Now.Minute) % 60;
                Logger.LogInformation("{0}, waiting until {1}, {2} minutes...", DateTimeOffset.Now, DateTime.Now.AddMinutes(waitMins), waitMins);
                await Task.Delay((int)TimeSpan.FromMinutes(waitMins).TotalMilliseconds, stoppingToken);
                try
                {
                    using (var http = new HttpClient())
                    {
                        var responseM = await http.GetAsync(endpointM);
                        var temp = JsonSerializer.Deserialize<Response>(
                            await responseM.Content.ReadAsStringAsync(), JsonOptions);
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
                            forecast.Add(new Data(DataType.actual, m.id, m.dt, 0, m.main, m.wind, m.clouds));
                            if (m.dt.Hour % 3 == 0)
                            {
                                string endpointF = $"http://api.openweathermap.org/data/2.5/forecast?id={m.id}&units=metric&appid={Config["OpenWeatherApiKey"]}";
                                var responseF = await http.GetAsync(endpointF);
                                var fc = JsonSerializer.Deserialize<Response>(await responseF.Content.ReadAsStringAsync(), JsonOptions);
                                forecast.AddRange(fc.list.Select(f => new Data(DataType.forecast, fc.city.id, f.dt, (int)(f.dt - m.dt).TotalMinutes, f.main, f.wind, f.clouds)));
                            }
                        }
                        forecast.RemoveAll(f => f.dt.CompareTo(DateTime.Now.AddDays(-4)) < 0);
                        await Cache.SetRecordAsync(RedisKeyM, trend);
                        await Cache.SetCompressedRecordAsync(RedisKeyF, forecast);
                        Logger.LogInformation("Saved {0} Trend Rows and {1} Forecast Rows.", trend.Count, forecast.Count);
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
