using Celin.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using OpenWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Celin.Server
{
    public class WeatherForecastService : WeatherForecast.WeatherForecastBase
    {
        readonly static object Layout = new
        {
            margin = new { l = 60, r = 40, t = 60, b = 0 },
            autosize = true,
            plot_bgcolor = "rgb(219,233,244)"
        };
        ForecastDataService FD { get; set; }
        public override async Task<ForecastResponse> GetWeatherForecasts(Empty request, ServerCallContext context)
        {
            var response = new ForecastResponse();

            await FD.GetForecastData();
            response.Data = Google.Protobuf.ByteString.CopyFrom(FD.ForecastBytes);

            return response;
        }
        public override async Task<RandomGraphResponse> GetRandomGraph(RandomGraphRequest request, ServerCallContext context)
        {
            var response = new RandomGraphResponse();

            try
            {
                await FD.GetForecastData();
                response.Location = (request.LastLocation + 1) % 4;
                var location = PlotFactory.Locations.ElementAt(response.Location);
                var forecast = Compressor.Unpack<IEnumerable<Data>>(FD.ForecastBytes);
                var data = PlotFactory
                            .CreateMaxMinArea(
                                forecast
                                .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast))
                                .GroupBy(f => f.dt + location.Value.Item1, f => f.main.temp)
                                .Select(f => (f.Key, f.Max(), f.Min())), "rgb(114,160,193)")
                            .Concat(
                                forecast
                                .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast))
                                .GroupBy(f => Math.Round(f.fdt / 180d), f => (f.dt + location.Value.Item1, f.main.temp))
                                .Select(f => PlotFactory
                                .CreatePlot(
                                    f.AsEnumerable(),
                                    new
                                    {
                                        name = $"{f.Key}h",
                                        marker = new
                                        {
                                            color = "rgb(70,130,180)",
                                            opacity = (90.0 - f.Key * 2.0) / 100.0,
                                            size = (int)f.Key / 2,
                                            symbol = "square"
                                        },
                                        showlegend = false,
                                        hoverinfo = "none"
                                    })))
                            .Concat(new[]
                            {
                                PlotFactory.CreateLine(forecast
                                .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast))
                                .GroupBy(f => f.dt + location.Value.Item1, f => f.main.temp)
                                .Select(f => (f.Key, f.Average())), new
                                {
                                    name = "Forecast Avg",
                                    line = new { shape = "spline",  color = "rgb(219,233,244)", dash = "dash" },
                                    showlegend = false,
                                    hoverinfo = "none"
                                }),
                                PlotFactory.CreateMarkersLine(
                                forecast
                                .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.actual))
                                .Select(f => (f.dt + location.Value.Item1, f.main.temp)),
                                new
                                {
                                    name = "Actual",
                                    marker = new
                                    {
                                        symbol = "diamond-open",
                                        color = "rgb(18,97,128)"
                                    },
                                    showlegend = false
                                }),
                                PlotFactory.CreateLine(
                                forecast
                                .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast))
                                .TakeLast(40)
                                .Prepend(forecast.Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.actual)).Last())
                                .Select(f => (f.dt + location.Value.Item1, f.main.temp)),
                                new
                                {
                                    name = "Forecast",
                                    line = new
                                    {
                                        color = "rgb(18,97,128)",
                                        dash = "dot",
                                        width = 3,
                                        shape = "spline"
                                    },
                                    showlegend = false
                                })
                            });
                response.Data = Google.Protobuf.ByteString.CopyFrom(Compressor.Pack(data));

                var last = forecast.Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.actual)).Last();
                var layout = PlotFactory.Merge(Layout, new
                {
                    title = new
                    {
                        text = $"{location.Value.Item2} {last.main.temp}&#xb0;C ({(last.dt + location.Value.Item1).ToShortTimeString()})",
                        font = new
                        {
                            color = "rgb(18,97,128)",
                            size = 28
                        }
                    },
                    margin = new { l = 60, r = 40, t = 60, b = 0 },
                    xaxis = new { side = "top" },
                    yaxis = new
                    {
                        title = "Temperature",
                        range = new []
                        {
                            forecast.Select(f => Math.Floor(f.main.temp)).Min() - 4,
                            forecast.Select(f => Math.Ceiling(f.main.temp)).Max() + 4
                        }
                    }
                });
                response.Layout = Google.Protobuf.ByteString.CopyFrom(Compressor.Pack(layout));
            }
            catch (Exception ex) { }

            return response;
        }
        public WeatherForecastService(ForecastDataService fd)
        {
            FD = fd;
        }
    }
}
