using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenWeather;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string endpoint = $"http://api.openweathermap.org/data/2.5/group?id=3413829,2618425,1566083,2063523&units=metric&appid=";
            var http = new HttpClient();
            var response = await http.GetAsync(endpoint);
            Console.WriteLine(response.Content.ReadAsStringAsync());
            var temp = JsonSerializer.Deserialize<Response>(
                await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
                {
                    Converters =
                    {
                        new DateJsonConverter(),
                        new TimeSpanConverter()
                    }
                });
        }
    }
}
