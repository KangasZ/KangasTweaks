using System;

namespace KangasTweaks.TrackerModule;

public class TrackerModule
{
    private readonly DateTime WeeklyResetTime = new DateTime(2024, 1, 23, 8, 0, 0, DateTimeKind.Utc);
    private readonly DateTime DailyResetTime = new DateTime(2024, 1, 22, 15, 0, 0, DateTimeKind.Utc);
    
    public TrackerModule()
    {
        
    }
    
    
}