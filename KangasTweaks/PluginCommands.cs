using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using KangasTweaks;
using KangasTweaks.TimerModule;
using KangasTweaks.TrackerModule;
using KangasTweaks.WeatherModule;

namespace KangasTweaks;

public class PluginCommands : IDisposable
{
    private readonly ICommandManager commandManager;
    private readonly WeatherUi weatherUi;
    private readonly Configuration configInterface;
    private readonly ResetTracker resetTracker;
    private readonly TimerManager timerManager;
    private readonly IChatGui chatGui;

    public PluginCommands(ICommandManager commandManager, WeatherUi weatherUi, Configuration configuration,
        ResetTracker resetTracker, TimerManager timerManager, IChatGui chatGui)
    {
        this.resetTracker = resetTracker;
        this.weatherUi = weatherUi;
        this.commandManager = commandManager;
        this.configInterface = configuration;
        this.chatGui = chatGui;
        this.timerManager = timerManager;
        this.commandManager.AddHandler("/kweather", new CommandInfo(WeatherCommand)
        {
            HelpMessage = "Opens Weather Menu",
            ShowInHelp = true
        });
        this.commandManager.AddHandler("/ktracker", new CommandInfo(TrackerCommand)
        {
            HelpMessage = "Opens Weekly and Daily Tracker Menu",
            ShowInHelp = true
        });
        this.commandManager.AddHandler("/ktimer", new CommandInfo(TimerCommand)
        {
            HelpMessage = "Timers Stuff Usage: /ktimer [minutes] (message)",
            ShowInHelp = true
        });
    }

    private void TimerCommand(string command, string args)
    {
        var regex = Regex.Match(args, "^(\\w+) ?(.*)");
        if (regex.Success && regex.Groups.Count > 1)
        {
            if (int.TryParse(regex.Groups[1].Value, out var minutes))
            {
                var timeIntervalForTimer = new TimeSpan(0, minutes, 0);
                IEnumerable<string> remainingGroups;
                if (regex.Groups.Count >= 2)
                {
                    remainingGroups = regex.Groups.Cast<Group>().Skip(2).Select(g => g.Value);
                }
                else
                {
                    remainingGroups = new[] { "Generic Timer Name" };
                }
                var message = string.Join(" ", remainingGroups);

                var seString = new SeStringBuilder();
                seString.Append($"{minutes} minute timer created with message: {message}");
                var chatEntry = new XivChatEntry()
                {
                    Type = XivChatType.Echo,
                    Message = seString.Build()
                };
                chatGui.Print(chatEntry);
                timerManager.CreateTimer(timeIntervalForTimer, message);
            }
            else
            {
                var seString = new SeStringBuilder();
                seString.Append("Timers Stuff Usage: /ktimer [minutes] (message)");
                var chatEntry = new XivChatEntry()
                {
                    Type = XivChatType.Echo,
                    Message = seString.Build()
                };
                chatGui.Print(chatEntry);
            }
        }
        else
        {
            var seString = new SeStringBuilder();
            seString.Append("Timers Stuff Usage: /ktimer [minutes] (message)");
            var chatEntry = new XivChatEntry()
            {
                Type = XivChatType.Echo,
                Message = seString.Build()
            };
            chatGui.Print(chatEntry);
        }
    }

    private void TrackerCommand(string command, string args)
    {
        resetTracker.ShowTrackerConfigWindow();
    }

    private void WeatherCommand(string command, string args)
    {
        weatherUi.OpenUi();
    }

    public void Dispose()
    {
        commandManager.RemoveHandler("/kweather");
        commandManager.RemoveHandler("/ktracker");
        commandManager.RemoveHandler("/ktimer");
    }
}