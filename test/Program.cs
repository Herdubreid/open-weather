using OpenWeather;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
        public static ExpandoObject CopyValues(object ob1, object ob2)
        {
            ExpandoObject result = new ExpandoObject();

            foreach (var prop in ob1.GetType().GetProperties())
            {
                var value = prop.GetValue(ob1, null);
                if (value != null)
                    result.TryAdd(prop.Name, value);
            }

            foreach (var prop in ob2.GetType().GetProperties())
            {
                var value = prop.GetValue(ob2, null);
                if (value != null)
                    result.TryAdd(prop.Name, value);
            }

            return result;
        }
        static async Task Main(string[] args)
        {
            try
            {
                var redislab = ConnectionMultiplexer
                    .Connect("redis");
                var db = redislab.GetDatabase();
                var last = await db.HashGetAllAsync("OpenWeather_Forecast");
                byte[] b = last[1].Value;

                var data = Compressor.Unpack<List<Data>>(b) ?? new List<Data>();

                var rek = data
                    .Where(f => f.id.Equals(2063523) && f.dtype.Equals(DataType.forecast))
                    .GroupBy(f => f.dt, f => f.main.temp);
                var list = new List<decimal?[]>(
                    Enumerable.Range(1, rek.Count()).Select(_ => new decimal?[40]));
                for (int row = 0; row < 40; row++)
                {
                    for (int col = 0; col < rek.ElementAt(row).Count(); col++)
                    {
                        list.ElementAt(col)[row] = rek.ElementAt(row).ElementAt(col);
                    }
                }
                decimal?[] lastRow = list.First();
                foreach (var row in list)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        row[i] = row[i] ?? lastRow[i];
                    }
                    lastRow = row;
                }
                foreach (var row in list)
                {
                    foreach (var col in row)
                    {
                        Console.Write("{0:0.0} ", col);
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
