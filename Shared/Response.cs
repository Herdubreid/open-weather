using System;

namespace OpenWeather
{
    public record Coord
    (
        (TimeSpan, decimal) lon,
        decimal lat
    );
    public record Sys
    (
        string country,
        int timezone,
        DateTime sunrise,
        DateTime sunset
    );
    public record Weather
    (
        int id,
        string main,
        string description,
        string icon
    );
    public record Main
    (
        decimal temp,
        decimal feels_like,
        decimal temp_min,
        decimal temp_max,
        int pressure,
        int humidity
    );
    public record Wind
    (
        decimal speed,
        int deg
    );
    public record Clouds
    (
        int all
    );
    public record Measure
    (
        Coord coord,
        Sys sys,
        Weather[] weather,
        Main main,
        int visibility,
        Wind wind,
        Clouds clouds,
        DateTime dt,
        int id,
        string name
    );
    public record City
    (
        int id,
        string name,
        Coord coord,
        string country,
        int population,
        int timezone,
        int sunrise,
        int sunset
    );
    public record Response
    (
        int cnt,
        Measure[] list,
        City city
    );
}
