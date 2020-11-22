using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenWeather;

namespace Celin.Server
{
    public class WeatherTrendService : WeatherTrends.WeatherTrendsBase
    {
        IDistributedCache Cache { get; }
        public override async Task<GetWeatherTrendResponse> GetWeatherTrends(Empty request, ServerCallContext context)
        {
            var response = new GetWeatherTrendResponse();
            try
            {
                var trends = await Cache.GetRecordAsync<Dictionary<int, List<Trend>>>("Trend");
                foreach (var trend in trends)
                {
                    var lwt = new LocationWeatherTrends { Location = trend.Key };
                    var l = trend.Value.Select(t =>
                    {
                        var utc = DateTime.SpecifyKind(t.TimeStamp, DateTimeKind.Utc);
                        return new WeatherTrend
                        {
                            Timestamp = Timestamp.FromDateTimeOffset(utc),
                            Temp = new Measure { Average = (int)Math.Round(t.Temp.Item1 * 10), Current = (int)Math.Round(t.Temp.Item2 * 10) },
                            FeelsLike = new Measure { Average = (int)Math.Round(t.FeelsLike.Item1 * 10), Current = (int)Math.Round(t.FeelsLike.Item2 * 10) },
                            Humidity = new Measure { Average = t.Humidity.Item1, Current = t.Humidity.Item2 },
                            Pressure = new Measure { Average = t.Pressure.Item1, Current = t.Pressure.Item2 }
                        };
                    });
                    lwt.Trends.Add(l);
                    response.LocationWeatherTrends.Add(lwt);
                }
            }
            catch { }

            return response;
        }
        public WeatherTrendService(IDistributedCache cache)
        {
            Cache = cache;
        }
    }
}
