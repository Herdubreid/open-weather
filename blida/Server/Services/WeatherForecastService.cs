using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace Celin.Server
{
    public class WeatherForecastService : WeatherForecast.WeatherForecastBase
    {
        IDistributedCache Cache { get; }
        public override async Task<ForecastResponse> GetWeatherForecasts(Empty request, ServerCallContext context)
        {
            var response = new ForecastResponse();
            try
            {
                var forecast = await Cache.GetAsync("Forecast");
                response.Data = Google.Protobuf.ByteString.CopyFrom(forecast);
            }
            catch { }

            return response;
        }
        public WeatherForecastService(IDistributedCache cache)
        {
            Cache = cache;
        }
    }
}
