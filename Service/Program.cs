using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(logger =>
                    {
                        logger
                            .AddConfiguration(hostContext.Configuration)
                            .AddConsole();
                    });
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = hostContext.Configuration.GetSection("ConnectionStrings")["Redis"];
                        options.InstanceName = "OpenWeather_";
                    });
                    services.AddHostedService<Worker>();
                });
    }
}
