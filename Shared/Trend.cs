using System;

namespace OpenWeather
{
    public class Trend
    {
        public int Count { get; set; }
        public DateTime TimeStamp { get; set; }
        public int MinShift { get; set; }
        public (decimal, decimal) Temp { get; set; }
        public (decimal, decimal) FeelsLike { get; set; }
        public (int, int) Pressure { get; set; }
        public (int, int) Humidity { get; set; }
        static public Trend Add(Trend trend, Measure measure)
        {
            if (trend is null)
            {
                return new Trend
                {
                    Count = 1,
                    TimeStamp = measure.dt,
                    MinShift = measure.coord.lon.Item1.Minutes,
                    Temp = (measure.main.temp, measure.main.temp),
                    FeelsLike = (measure.main.feels_like, measure.main.feels_like),
                    Pressure = (measure.main.pressure, measure.main.pressure),
                    Humidity = (measure.main.humidity, measure.main.humidity)
                };
            }
            else
            {
                var w = (decimal)1 / (trend.Count + 1);
                return new Trend
                {
                    Count = trend.Count + 1,
                    TimeStamp = measure.dt,
                    MinShift = measure.coord.lon.Item1.Minutes,
                    Temp = (trend.Temp.Item1 + (measure.main.temp - trend.Temp.Item1) * w, measure.main.temp),
                    FeelsLike = (trend.FeelsLike.Item1 + (measure.main.feels_like - trend.FeelsLike.Item1) * w, measure.main.feels_like),
                    Pressure = (trend.Pressure.Item1 + (int)Math.Round((measure.main.pressure - trend.Pressure.Item1) * w), measure.main.pressure),
                    Humidity = (trend.Humidity.Item1 + (int)Math.Round((measure.main.humidity - trend.Humidity.Item1) * w), measure.main.humidity)
                };
            }
        }
    }
}
