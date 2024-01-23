using System.Collections.Generic;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KangasTweaks.TrackerModule;
using KangasTweaks.WeatherModule;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace KangasTweaks;

public class KangasTweaks : IDalamudPlugin
{
    public string Name => "Kangas Tweaks";

    private WeatherUi weatherUi;
    private readonly Configuration configuration;
    private readonly DalamudPluginInterface pluginInterface;
    private readonly PluginCommands pluginCommands;

    private readonly ImageStore imageStore;

    private readonly IDataManager dataManager;

    private readonly WeatherManager weatherModule;

    private readonly ResetTracker resetTracker;
    //private readonly 

    public KangasTweaks(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IObjectTable objectTable,
        ICondition condition,
        IClientState clientState,
        IGameGui gameGui,
        IDataManager dataManager,
        ITextureProvider textureProvider)
    {
        this.pluginInterface = pluginInterface;
        this.configuration = this.pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.configuration.Initialize(this.pluginInterface);
        this.dataManager = dataManager;
        this.weatherModule = new WeatherManager(dataManager);
        this.imageStore = new ImageStore(pluginInterface, textureProvider);
        this.resetTracker = new ResetTracker(pluginInterface, this.configuration);
        
        this.weatherUi = new WeatherUi(pluginInterface, configuration, clientState, this.imageStore, this.weatherModule);
        this.pluginCommands = new PluginCommands(commandManager, weatherUi, configuration, this.resetTracker);
        // No fail check
    }


    public void Dispose()
    {
        resetTracker.Dispose();
        weatherUi.Dispose();
        pluginCommands.Dispose(); 
    }
}