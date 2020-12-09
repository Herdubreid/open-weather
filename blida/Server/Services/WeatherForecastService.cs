using Celin.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OpenWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Celin.Server
{
    public class WeatherForecastService : WeatherForecast.WeatherForecastBase
    {
        ILogger<WeatherForecastService> Logger { get; }
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
        public override async Task GetAnimatedGraph(GraphRequest request, IServerStreamWriter<GraphResponse> responseStream, ServerCallContext context)
        {
            try
            {
                var location = PlotFactory.Locations.ElementAt(request.LastLocation);
                await FD.GetForecastData();
                var forecast = Compressor.Unpack<IEnumerable<Data>>(FD.ForecastBytes);

                var lastTemp = forecast
                    .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.actual))
                    .Last();

                var xaxis = forecast
                    .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast) && (f.dt.CompareTo(lastTemp.dt) < 0))
                    .Select(f => f.dt)
                    .Distinct();

                var rek = forecast
                    .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast) && (f.dt.CompareTo(lastTemp.dt) < 0))
                    .GroupBy(f => f.dt, f => f.main.temp);

                var list = new List<decimal?[]>(
                    Enumerable.Range(0, 40).Select(_ => new decimal?[40]));
                for (int row = 0; row < rek.Count(); row++)
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

                await responseStream.WriteAsync(
                    new GraphResponse
                    {
                        Data = Google.Protobuf.ByteString.CopyFrom(
                            Compressor.Pack(new[]
                            {
                                new
                                {
                                    x = xaxis.ToArray(),
                                    y = list.First(),
                                    name = "Forecast",
                                    mode = "lines+markers",
                                    type = "scatter",
                                    line = new
                                    {
                                        shape = "spline"
                                    },
                                    showlegend = false
                                },
                                PlotFactory.CreateMarkersLine(
                                    forecast
                                    .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.actual))
                                    .Select(f => (f.dt, f.main.temp)),
                                new
                                {
                                    name = "Actual",
                                    marker = new
                                    {
                                        symbol = "diamond-open",
                                        color = "rgb(204,153,0)"
                                    },
                                    showlegend = false
                                })
                            })),
                            Layout = Google.Protobuf.ByteString.CopyFrom(
                                Compressor.Pack(new
                                {
                                    title = location.Value.Item2,
                                    yaxis = new
                                    {
                                        title = "Temperature",
                                        range = new[]
                                        {
                                            forecast
                                            .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast))
                                            .Select(f => Math.Floor(f.main.temp)).Min() - 4,
                                            forecast
                                            .Where(f => f.id.Equals(location.Key) && f.dtype.Equals(DataType.forecast))
                                            .Select(f => Math.Ceiling(f.main.temp)).Max() + 4
                                        }
                                    }
                                }))
                            });
                foreach (var next in list.Skip(1))
                {
                    await Task.Delay(1500);

                    await responseStream.WriteAsync(
                        new GraphResponse
                        {
                            Data = Google.Protobuf.ByteString.CopyFrom(
                                Compressor.Pack(new
                                {
                                    data = new[] { new { y = next } },
                                    traces = new[] { 0 }
                                }))
                        });
                }
                Logger.LogInformation("Finished!");
            }
            catch (Exception ex) { }
        }
        public override async Task<GraphResponse> GetRandomGraph(GraphRequest request, ServerCallContext context)
        {
            var response = new GraphResponse();

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
                        range = new[]
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
        public WeatherForecastService(ILogger<WeatherForecastService> logger, ForecastDataService fd)
        {
            Logger = logger;
            FD = fd;
        }
    }
}
