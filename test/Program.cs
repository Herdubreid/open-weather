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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;

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
                var redislab = ConnectionMultiplexer
                    .Connect("redis-15134.c244.us-east-1-2.ec2.cloud.redislabs.com:15134,password=");
                var db2 = redislab.GetDatabase();
                var old = await db2.HashGetAllAsync("OpenWeather_Forecast");
                byte[] b = old[1].Value;

                var last = Compressor.Unpack<List<Data>>(b) ?? new List<Data>();
                foreach (var t in last
                    .Where(f => f.id.Equals(2063523) && f.dtype.Equals(DataType.forecast))
                    .GroupBy(f => f.dt, f => f.main.temp)
                    .Select(f => (f.Key, f.Average(), f.Max(), f.Min())))
                {
                    Console.WriteLine("{0} {1:0.0} {2:0.0} {3:0.0}", t.Item1, t.Item2, t.Item3, t.Item4);
                }
                var http = new HttpClient();
                string endpoint = "http://api.openweathermap.org/data/2.5/group?id=3413829&units=metric&appid=";
                var response = await http.GetAsync(endpoint);
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                var temp = JsonSerializer.Deserialize<Response>(
                    await response.Content.ReadAsStringAsync(), JsonOptions);
                List<Data> data = temp.list.Select(t => new Data(
                    DataType.actual, t.id, t.dt, 0, t.main, t.wind, t.clouds))
                    .ToList();
                var dt = data.First().dt;

                endpoint = "http://api.openweathermap.org/data/2.5/forecast?id=3413829&units=metric&appid=";
                response = await http.GetAsync(endpoint);
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                temp = JsonSerializer.Deserialize<Response>(
                    await response.Content.ReadAsStringAsync(), JsonOptions);
                data.AddRange(temp.list.Select(t => new Data(
                    DataType.forecast, temp.city.id, t.dt, (int)(t.dt - dt).TotalMinutes, t.main, t.wind, t.clouds)
                ));
                data.Sort(DataComparer.ByDt);
                foreach (var t in data)
                {
                    Console.WriteLine("{0} {1} {2} {3} {4} {5}", t.dtype.ToString("g"), t.id, t.dt, t.fdt, t.main.temp, t.GetHashCode());
                }
                /*
                var diff = data.Except(last);
                foreach (var t in diff)
                {
                    Console.WriteLine("{0} {1} {2} {3}", t.dtype.ToString("g"), t.id, t.dt, t.main.temp);
                }

                var b = Compressor.Pack<IEnumerable<Data>>(data);

                Console.WriteLine("Old: {0}", b.Length);
                await db2.StringSetAsync("TestKey", b);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
