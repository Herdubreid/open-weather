using System;
using System.Collections.Generic;
using System.Linq;

namespace Celin.Client
{
    public static class PlotFactory
    {
        static TimeSpan LocalOffset { get; } = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        public static Dictionary<int, (TimeSpan, string)> Locations { get; } = new Dictionary<int, (TimeSpan, string)>
        {
            { 3413829, new (TimeSpan.FromMinutes(-87), "REK") },
            { 2618425, new (TimeSpan.FromMinutes(51), "CPH") },
            { 1566083, new (TimeSpan.FromMinutes(426), "SGN") },
            { 2063523, new (TimeSpan.FromMinutes(463), "PER") }
        };
        public static object CreatePlot<T1, T2>(IEnumerable<(T1, T2)> data, object options)
            => TypeMerger.TypeMerger.Merge(new
            {
                x = data.Select(d => d.Item1),
                y = data.Select(d => d.Item2),
                mode = "markers",
                type = "scatter"
            }, options);
        public static IEnumerable<object> CreatePlot<T1, T2, T3>(IEnumerable<IGrouping<T1, (T2, T3)>> data)
            => data.Select(d => CreatePlot(d.AsEnumerable(), new { }));
        public static object[] CreateAverageLine<T1, T2, T3, T4>(IEnumerable<(T1, T2, T3, T4)> data, string fillcolor)
            => new[]
            {
                CreateLine(data.Select(f => (f.Item1, f.Item4)), new
                {
                    name = "Forecast Min",
                    line = new { width = 0, shape = "spline" },
                    marker = new { color = "444" },
                    showlegend = false,
                    hoverinfo = "none"
                }),
                CreateLine(data.Select(f => (f.Item1, f.Item2)), new
                {
                    name = "Forecast Avg",
                    fill = "tonexty",
                    fillcolor = fillcolor,
                    line = new { shape = "spline",  color = "rgb(219,233,244)", dash = "dash" },
                    marker = new { color = "444" },
                    showlegend = false,
                    hoverinfo = "none"
                }),
                CreateLine(data.Select(f => (f.Item1, f.Item3)), new
                {
                    name = "Max",
                    fill = "tonexty",
                    fillcolor = fillcolor,
                    line = new { width = 0, shape = "spline" },
                    marker =  new { color = "444" },
                    showlegend = false,
                    hoverinfo = "none"
                })
            };
        public static object CreateLine<T1, T2>(IEnumerable<(T1, T2)> data, object options)
            => TypeMerger.TypeMerger.Merge(new
            {
                x = data.Select(d => d.Item1),
                y = data.Select(d => d.Item2),
                mode = "lines",
                type = "scatter"
            }, options);
        public static object CreateMarkersLine<T1, T2>(IEnumerable<(T1, T2)> data, object options)
            => TypeMerger.TypeMerger.Merge(new
            {
                x = data.Select(d => d.Item1),
                y = data.Select(d => d.Item2),
                mode = "lines+markers",
                type = "scatter",
                line = new { shape = "spline" }
            }, options);
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
