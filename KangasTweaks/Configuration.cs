using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using KangasTweaks.Constants;

namespace KangasTweaks;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public class TrackerSettings
    {
        public bool Enabled = true;
        // Fix all the presets
        public uint ForegroundColor = ConfigConstants.Blue & ConfigConstants.Opacity60Percent;
        public uint BackgroundColor = ConfigConstants.Black & ConfigConstants.Opacity60Percent;
        public uint TextColor = ConfigConstants.White;
        public uint BorderColor = ConfigConstants.Black & ConfigConstants.Opacity80Percent;
        public float BorderThickness = ConfigConstants.DefaultBorderThickness;
        public float Width = ConfigConstants.DefaultTrackerWidth;
        public float Height = ConfigConstants.DefaultTrackerHeight;
    }

    public int Version { get; set; } = 0;
    public uint CurrentZoneValIndex { get; set; } = 0;
    public bool OnlyShowFavoritedZones { get; set; } = false;
    public List<uint> FavoritedZones { get; set; } = new();
    public TrackerSettings WeeklyTrackerSettings = new();
    public TrackerSettings DailyTrackerSettings = new();
    
    [NonSerialized] private DalamudPluginInterface? pluginInterface;
    
    

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }

}