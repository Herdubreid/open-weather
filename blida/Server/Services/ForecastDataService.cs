using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Celin.Server
{
    public class ForecastDataService
    {
        IDistributedCache Cache { get; }
        public byte[] ForecastBytes { get; set; }
        DateTime Updated { get; set; }
        bool TimeForUpdate
        {
            get
            {
                if (ForecastBytes is null)
                {
                    return true;
                }
                var updateMin = (90 - DateTime.Now.Minute) % 60;
                var lastUpdate = DateTime.Now.AddMinutes(updateMin - 60);
                if (Updated.CompareTo(lastUpdate) < 1)
                {
                    return true;
                }

                return false;
            }
        }
        public async Task GetForecastData()
        {
            try
            {
                if (TimeForUpdate)
                {
                    ForecastBytes = await Cache.GetAsync("Forecast");
                    Updated = DateTime.Now;
                }
            }
            catch { }
        }
        public ForecastDataService(IDistributedCache cache)
        {
            Cache = cache;
        }
    }
}
