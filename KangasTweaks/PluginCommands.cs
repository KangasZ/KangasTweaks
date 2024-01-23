﻿using System;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using KangasTweaks;
using KangasTweaks.WeatherModule;

namespace KangasTweaks;

public class PluginCommands : IDisposable
{
    private readonly ICommandManager commandManager;
    private readonly WeatherUi weatherUi;
    private readonly Configuration configInterface;

    public PluginCommands(ICommandManager commandManager, WeatherUi weatherUi, Configuration configuration)
    {
        this.weatherUi = weatherUi;
        this.commandManager = commandManager;
        this.configInterface = configuration;
        this.commandManager.AddHandler("/kweather", new CommandInfo(WeatherCommand)
        {
            HelpMessage = "Opens Weather Menu",
            ShowInHelp = true
        });
    }

    private void WeatherCommand(string command, string args)
    {
        weatherUi.OpenUi();
        return;
        if (string.IsNullOrEmpty(args))
        {
            weatherUi.OpenUi();
            return;
        }
        /*
        var regex = Regex.Match(args, "^(\\w+) ?(.*)");
        var subcommand = regex.Success && regex.Groups.Count > 1 ? regex.Groups[1].Value : string.Empty;
        switch (subcommand.ToLower())
        {
            case "debug":
            {
                configInterface.cfg.DebugMode = !configInterface.cfg.DebugMode;
                break;
            }
        }*/
    }

    public void Dispose()
    {
        commandManager.RemoveHandler("/kweather");
    }
}