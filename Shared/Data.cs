using System;

namespace OpenWeather
{
    public static class DataComparer
    {
        public static int ByDt(Data d1, Data d2)
        {
            return DateTime.Compare(d1.dt, d2.dt);
        }
    }
    public enum DataType
    {
        actual,
        forecast
    }
    public record Data
    (
        DataType dtype,
        int id,
        DateTime dt,
        int fdt,
        Main main,
        Wind wind,
        Clouds clouds
    );
}
