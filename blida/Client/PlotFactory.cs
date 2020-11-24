using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Celin.Client
{
    public static class PlotFactory
    {
        static TimeSpan LocalOffset { get; } = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        static Dictionary<int, (TimeSpan, string)> Locations { get; } = new Dictionary<int, (TimeSpan, string)>
        {
            { 3413829, new (TimeSpan.FromMinutes(-87), "REK") },
            { 2618425, new (TimeSpan.FromMinutes(51), "CPH") },
            { 1566083, new (TimeSpan.FromMinutes(426), "SGN") },
            { 2063523, new (TimeSpan.FromMinutes(463), "PER") }
        };
        public static object CreateTempGraph(IEnumerable<LocationWeatherTrends> locations)
            => locations.Select(l => new
            {
                x = l.Trends.Select(t => ((t.Timestamp.ToDateTimeOffset() - DateTime.Now - LocalOffset).Add(Locations[l.Location].Item1).TotalHours)),
                y = l.Trends.Select(t => t.Temp.Current / 10.0),
                mode = "lines+markers",
                type = "scatter",
                line = new { shape = "spline" },
                name = Locations[l.Location].Item2,
                text = l.Trends.Select(t => t.Timestamp.ToDateTimeOffset().Add(Locations[l.Location].Item1).ToString("t")),
                error_y = new
                {
                    type = "data",
                    symmetric = false,
                    array = l.Trends.Select(t => t.FeelsLike.Current > t.Temp.Current ? (t.FeelsLike.Current - t.Temp.Current) / 10.0 : 0.0),
                    arrayminus = l.Trends.Select(t => t.FeelsLike.Current < t.Temp.Current ? (t.Temp.Current - t.FeelsLike.Current) / 10.0 : 0.0)
                }
            });
        public static object CreateHumidityGraph(IEnumerable<LocationWeatherTrends> locations)
            => locations.Select(l => new
            {
                x = l.Trends.Select(t => ((t.Timestamp.ToDateTimeOffset().LocalDateTime - DateTime.Now - LocalOffset).Add(Locations[l.Location].Item1).TotalHours)),
                y = l.Trends.Select(t => t.Humidity.Current),
                mode = "lines+markers",
                type = "scatter",
                line = new { shape = "spline" },
                name = Locations[l.Location].Item2,
                text = l.Trends.Select(t => t.Timestamp.ToDateTimeOffset().Add(Locations[l.Location].Item1).ToString("t")),
            });
        public static object CreatePressureGraph(IEnumerable<LocationWeatherTrends> locations)
            => locations.Select(l => new
            {
                x = l.Trends.Select(t => ((t.Timestamp.ToDateTimeOffset().LocalDateTime - DateTime.Now - LocalOffset).Add(Locations[l.Location].Item1).TotalHours)),
                y = l.Trends.Select(t => t.Pressure.Current),
                mode = "lines+markers",
                type = "scatter",
                line = new { shape = "spline" },
                name = Locations[l.Location].Item2,
                text = l.Trends.Select(t => t.Timestamp.ToDateTimeOffset().Add(Locations[l.Location].Item1).ToString("t")),
            });
    }
}
