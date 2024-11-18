using System;
using System.Collections.Generic;
using System.Timers;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace KangasTweaks.TimerModule;

public class TimerManager
{
    private readonly Dictionary<int, TimerWrapper> timers = new Dictionary<int, TimerWrapper>();
    private readonly IChatGui chatGui;
    private static int timerId = 0;
    private readonly IPluginLog pluginLog;

    public TimerManager(IChatGui chatGui, IPluginLog pluginLog)
    {
        this.chatGui = chatGui;
        this.pluginLog = pluginLog;
    }

    public void CreateTimer(TimeSpan interval, string description)
    {
        timerId += 1;
        if (timers.ContainsKey(timerId))
        {
            throw new ArgumentException($"Timer with id {timerId} already exists. How did you reach this? Like seriously? What are you doing? What happened? Whatever it's not my place in life to judge.");
        }
        var timer = new TimerWrapper(interval, description, timerId);
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = true;
        timers[timerId] = timer;
    }

    private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        var activatedString = "Meow";
        if (source is TimerWrapper timer)
        {
            timer.Stop();
            UIGlobals.PlayChatSoundEffect(1);
            UIGlobals.PlayChatSoundEffect(2);
            UIGlobals.PlayChatSoundEffect(3);
            UIGlobals.PlayChatSoundEffect(4);
            UIGlobals.PlayChatSoundEffect(5);
            UIGlobals.PlayChatSoundEffect(6);
            UIGlobals.PlayChatSoundEffect(7);
            UIGlobals.PlayChatSoundEffect(8);
            UIGlobals.PlayChatSoundEffect(9);
            UIGlobals.PlayChatSoundEffect(10);
            UIGlobals.PlayChatSoundEffect(11);

            
            var seString = new SeStringBuilder();
            seString.Append($"KTweaks Timers: {timer.Description}");
            var chatEntry = new XivChatEntry()
            {
                Type = XivChatType.Echo,
                Message = seString.Build()
            };
            chatGui.Print(chatEntry);

            if (!timers.Remove(timer.Id))
            {
                pluginLog.Error("Failed to remove timer from dictionary");
            }
            timer.Close();
        }
    }
}