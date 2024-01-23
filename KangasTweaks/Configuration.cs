using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace KangasTweaks;

[Serializable]
public class Configuration : IPluginConfiguration
{

    public int Version { get; set; } = 0;
    public uint CurrentZoneValIndex { get; set; } = 0;
    public bool OnlyShowFavoritedZones { get; set; } = false;
    public List<uint> FavoritedZones { get; set; } = new();
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