using System;

namespace OpenWeather
{
    public class Coord
    {
        public (TimeSpan, decimal) lon { get; set; }
        public decimal lat { get; set; }
    }
    public class Sys
    {
        public string country { get; set; }
        public int timezone { get; set; }
        public DateTime sunrise { get; set; }
        public DateTime sunset { get; set; }
    }
    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    public class Main
    {
        public decimal temp { get; set; }
        public decimal feels_like { get; set; }
        public decimal temp_min { get; set; }
        public decimal temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }
    public class Wind
    {
        public decimal speed { get; set; }
        public int deg { get; set; }
    }
    public class Clouds
    {
        public int all { get; set; }
    }
    public class Measure
    {
        public Coord coord { get; set; }
        public Sys sys { get; set; }
        public Weather[] weather { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public DateTime dt { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Response
    {
        public int cnt { get; set; }
        public Measure[] list { get; set; }
    }
}
