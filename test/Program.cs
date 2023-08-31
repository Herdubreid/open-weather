using OpenWeather;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        readonly static int REK = 3413829;
        readonly static int CPH = 2618425;
        readonly static int SGN = 1566083;
        readonly static int PER = 2063523;

        static List<Data> Data;
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
        static void WriteTemp(int loc)
        {
            var d = from row in Data
                    where row.id.Equals(loc) && row.dtype.Equals(DataType.actual)
                    select row;

            var temps = d
                      .Select(actual => actual.main.temp)
                      .Prepend(0)
                      .ToArray();
            var avg = Math.Round(temps
                .Skip(1)
                .Average(), 1);
            var change = d
                .Select((row, i) =>
                    (
                    row.main.temp,
                    change: row.main.temp - temps[i],
                    meandev: row.main.temp - avg
                    ))
                .ToArray();

            var sw = new StreamWriter($"l{loc}.csv");
            sw.WriteLine("temp,change,meandev");
            foreach (var row in change.Skip(1)) sw.WriteLine("{0},{1},{2}", row.temp, row.change, row.meandev);
            sw.Close();
        }
        static async Task Main(string[] args)
        {
            try
            {
                var redislab = ConnectionMultiplexer
                    .Connect("redis-15134.c244.us-east-1-2.ec2.cloud.redislabs.com:15134,password=Ya6bUVS2T5TtTGhafZPFWTEcOrtENMOp");
                var db = redislab.GetDatabase();
                var last = await db.HashGetAllAsync("OpenWeather_Forecast");
                byte[] b = last[1].Value;

                Data = Compressor.Unpack<List<Data>>(b) ?? new List<Data>();

                WriteTemp(REK);
                WriteTemp(CPH);
                WriteTemp(SGN);
                WriteTemp(PER);

                /*
                Console.ReadKey();
                var lastTemp = data
                    .Where(f => f.id.Equals(2063523) && f.dtype.Equals(DataType.actual))
                    .Last();

                var rek = data
                    .Where(f => f.id.Equals(2063523) && f.dtype.Equals(DataType.forecast) && (f.dt.CompareTo(lastTemp.dt) < 0))
                    .GroupBy(f => f.dt, f => f.main.temp);
                var list = new List<decimal?[]>(
                    Enumerable.Range(0, 40).Select(_ => new decimal?[40]));
                for (int row = 0; row < list.Count; row++)
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
                    Console.WriteLine("\n{0} Points\n", row.Length);
                }
                Console.WriteLine("{0} Rows", list.Count);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
