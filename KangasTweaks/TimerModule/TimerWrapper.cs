using System;
using System.Timers;

namespace KangasTweaks.TimerModule;

public class TimerWrapper : Timer
{
    public string Description;
    public int Id;
    public TimerWrapper(double interval, string description, int id) : base(interval)
    {
        Description = description;
        Id = id;
    }
    
    public TimerWrapper(TimeSpan interval, string description, int id) : base(interval)
    {
        Description = description;
        Id = id;
    }
}