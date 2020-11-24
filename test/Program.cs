using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenWeather;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace test
{
    class Program
    {
        static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            Converters =
            {
                new DateJsonConverter(),
                new TimeSpanConverter()
            }
        };
        static async Task Main(string[] args)
        {
            try
            {
                var waitMinutes = (85 - DateTime.Now.Minute) % 60;
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                Console.WriteLine("{0} {1} {2} {3}", DateTime.Now.Minute, waitMinutes, DateTime.Now.AddMinutes(waitMinutes), offset);
                /*
                string endpoint = "http://api.openweathermap.org/data/2.5/group?id=3413829,2618425,1566083,2063523&units=metric&appid=";
                var http = new HttpClient();
                var response = await http.GetAsync(endpoint);
                var temp = JsonSerializer.Deserialize<Response>(
                    await response.Content.ReadAsStringAsync(), JsonOptions);
                foreach (var t in temp.list)
                {
                    Console.WriteLine("{0} {1} {2} {3}", t.name, t.dt, t.main.feels_like, t.main.temp);
                }
                var redislab = ConnectionMultiplexer
                    .Connect("redis");
                var keilir = ConnectionMultiplexer.Connect("keilir");
                var db = keilir.GetDatabase();
                var ow = db.HashGetAll("OpenWeather_Trend");
                var db2 = redislab.GetDatabase();
                string value = "abcdef";
                await db2.StringSetAsync("TestKey", value);
                await db2.HashSetAsync("OpenWeather_Trend", ow);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
