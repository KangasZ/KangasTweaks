using System;
using System.Collections.Generic;
using System.Linq;

namespace KangasTweaks;

public class EorzeaWeather
{
    public const float EIGHT_EORZEAN_HOURS_IN_IRL_SECONDS = 175 * 8;
    public const double MULTIPLIER = 144D / 7D;
    public static readonly DateTime ZeroDay = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    public static DateTime ToEorzeaTime(DateTime dateTime)
    {
        var utcTicks = dateTime.ToUniversalTime().Ticks - ZeroDay.Ticks;
        var eorzeaTicks = utcTicks * MULTIPLIER;
        return new DateTime((long)eorzeaTicks);
    }
    
    public static DateTime FromEorzeaTime(DateTime dateTime)
    {
        var convertedUtcTicks = dateTime.Ticks / MULTIPLIER;
        var irlTicks = convertedUtcTicks + ZeroDay.Ticks;
        return new DateTime((long)irlTicks);
    }

    public static int CalculateChance(DateTime irlTime)
    {
        var unix = (int)(irlTime.ToUniversalTime() - ZeroDay).TotalSeconds;
        var bell = unix / 175;
        var increment = ((uint)(bell + 8 - (bell % 8))) % 24;

        var totalDays = (uint)(unix / 4200);
        var calcBase = (totalDays * 100) + increment;

        var step1 = (calcBase << 11) ^ calcBase;
        var step2 = (step1 >> 8) ^ step1;

        return (int)(step2 % 100);
    }
    
    public static uint Forecast(IEnumerable<(uint, uint)> weatherCollection, int weatherChance) => weatherCollection.Where(x => weatherChance < x.Item1).Select(x => x.Item2).FirstOrDefault();

    public static DateTime LastWeatherIntervalFromIrlTime(DateTime irlTime)
    {
        var timeDifference = (irlTime.ToUniversalTime() - ZeroDay).TotalMilliseconds % (EIGHT_EORZEAN_HOURS_IN_IRL_SECONDS*1000);

        var newTime = irlTime.AddMilliseconds(-MathF.Floor((float)timeDifference) + 1000);
        return newTime;
    }
}