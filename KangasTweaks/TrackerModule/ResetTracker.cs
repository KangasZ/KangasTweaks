using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using KangasTweaks.Constants;

namespace KangasTweaks.TrackerModule;

public class ResetTracker : IDisposable
{
    //private readonly DateTime WeeklyResetTime = new DateTime(2024, 1, 23, 8, 0, 0, DateTimeKind.Utc); 8AM UTC, Tuesday
    //private readonly DateTime DailyResetTime = new DateTime(2024, 1, 23, 15, 0, 0, DateTimeKind.Utc); 3PM UTC, Daily
    private readonly Configuration configuration;
    private bool configWindowVisible = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;

    public ResetTracker(DalamudPluginInterface dalamudPluginInterface, Configuration configuration)
    {
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.configuration = configuration;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawConfigUi;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawTrackers;
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawConfigUi;
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawTrackers;
    }

    public void DrawTrackers()
    {
        if (!configuration.DailyTrackerSettings.Enabled && !configuration.WeeklyTrackerSettings.Enabled) return;
        var flags = configuration.Clickthrough
            ? ImGuiWindowFlags.NoMove |
              ImGuiWindowFlags.NoInputs |
              ImGuiWindowFlags.NoScrollbar |
              ImGuiWindowFlags.NoMouseInputs |
              ImGuiWindowFlags.NoScrollWithMouse |
              ImGuiWindowFlags.NoTitleBar |
              ImGuiWindowFlags.NoBringToFrontOnFocus |
              ImGuiWindowFlags.NoResize |
              ImGuiWindowFlags.NoNav |
              ImGuiWindowFlags.NoDecoration |
              ImGuiWindowFlags.NoDocking |
              ImGuiWindowFlags.NoFocusOnAppearing
            : ImGuiWindowFlags.NoTitleBar;
        flags |= configuration.ShowBackground ? ImGuiWindowFlags.NoBackground : ImGuiWindowFlags.None;
        if (configuration.SeparateTrackers)
        {
            if (configuration.WeeklyTrackerSettings.Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(configuration.WeeklyTrackerSettings.Width+100, configuration.DailyTrackerSettings.Height+100), ImGuiCond.Appearing);
            
                ImGui.Begin("KangasTweaksTrackerOverlayWeeklyTrackerOnly", flags);
                DrawWeeklyResetBar();
                ImGui.End();
            }

            if (configuration.DailyTrackerSettings.Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(configuration.DailyTrackerSettings.Width+100, configuration.DailyTrackerSettings.Height+100), ImGuiCond.Appearing);
            
                ImGui.Begin("KangasTweaksTrackerOverlayDailyTrackerOnly", flags);
                DrawDailyResetBar();
                ImGui.End();
            }
        }
        else
        {
            ImGui.SetNextWindowSize(new Vector2(Math.Max(configuration.DailyTrackerSettings.Width, configuration.WeeklyTrackerSettings.Width) + 100, 
                Math.Max(configuration.DailyTrackerSettings.Height, configuration.DailyTrackerSettings.Height) + 100), ImGuiCond.Appearing);
            
            ImGui.Begin("KangasTweaksTrackerOverlay", flags);

            //drawListPtr = ImGui.GetWindowDrawList();
            if (configuration.WeeklyTrackerSettings.Enabled)
            {
                DrawWeeklyResetBar();
            }
            
            if (configuration.DailyTrackerSettings.Enabled)
            {
                DrawDailyResetBar(); 
            }
            
            ImGui.End();
        }
    }

    public void ShowTrackerConfigWindow()
    {
        configWindowVisible = true;
    }

    public void DrawConfigUi()
    {
        if (!configWindowVisible)
        {
            return;
        }

        var size = new Vector2(400, 400);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));

        if (ImGui.Begin("Kangas Tweaks Tracker##reset-tracker-config", ref configWindowVisible))
        {
            DrawAllTrackerSettings();
            var nextWeeklyResetTime = NextWeeklyResetTime();
            var timeUntilWeeklyReset = nextWeeklyResetTime - DateTime.UtcNow;
            var nextDailyResetTime = NextDailyResetTime();
            var timeUntilDailyReset = nextDailyResetTime - DateTime.UtcNow;
            ImGui.Text("Weekly Reset Time: " + nextWeeklyResetTime.ToLocalTime());
            ImGui.Text("Time until weekly reset: " + timeUntilWeeklyReset.ToString(@"dd\:hh\:mm\:ss"));
            ImGui.Text("Daily Reset Time: " + nextDailyResetTime.ToLocalTime());
            ImGui.Text("Time until daily reset: " + timeUntilDailyReset.ToString(@"hh\:mm\:ss"));


            // Update the progress bars with the new progress values
            DrawWeeklyResetBar();
            DrawDailyResetBar();
            // Draw the tracker settings
            DrawTrackerSettings();
        }

        ImGui.End();
    }

    private void DrawAllTrackerSettings()
    {
        var shouldSave = false;
        shouldSave |= UiHelpers.DrawCheckbox("Separate Weekly and Daily Trackers", ref configuration.SeparateTrackers,
            "If enabled, the weekly and daily trackers will be in separate boxes.");

        shouldSave |= UiHelpers.DrawCheckbox("Clickthrough", ref configuration.Clickthrough,
            "If enabled, the tracker will be clickthrough and will not block mouse clicks.");
        
        shouldSave |= UiHelpers.DrawCheckbox("Show Background", ref configuration.ShowBackground,
            "If enabled, the tracker will have a background.");

        if (shouldSave)
        {
            configuration.Save();
        }
    }

    private DateTime NextDailyResetTime()
    {
        var now = DateTime.UtcNow;
        var nextDailyResetTime = now.Date.AddHours(15);
        if (now.Hour >= 15)
        {
            nextDailyResetTime = nextDailyResetTime.AddDays(1);
        }

        return nextDailyResetTime;
    }

    public void DrawDailyResetBar()
    {
        // Calculate the time remaining until the next daily reset
        var now = DateTime.UtcNow;
        var nextDailyResetTime = NextDailyResetTime();

        var timeUntilDailyReset = nextDailyResetTime - now;

        // Calculate the progress as a percentage of the week and day
        var dailyProgress = 1 - (timeUntilDailyReset.TotalSeconds / (24 * 60 * 60));
        UiHelpers.BufferingBar($"Daily Reset: {timeUntilDailyReset.ToString("hh':'mm':'ss")}",
            configuration.DailyTrackerSettings.BackgroundColor,
            configuration.DailyTrackerSettings.ForegroundColor, configuration.DailyTrackerSettings.BorderColor,
            configuration.DailyTrackerSettings.TextColor,
            configuration.DailyTrackerSettings.Width, configuration.DailyTrackerSettings.Height,
            configuration.DailyTrackerSettings.BorderThickness, (float)dailyProgress);
    }

    private DateTime NextWeeklyResetTime()
    {
        var now = DateTime.UtcNow;
        var daysUntilNextTuesday = ((int)DayOfWeek.Tuesday - (int)now.DayOfWeek + 7) % 7;
        if (now.DayOfWeek == DayOfWeek.Tuesday)
        {
            daysUntilNextTuesday = 7;
        }

        var nextWeeklyResetTime =
            new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc).AddDays(daysUntilNextTuesday);
        return nextWeeklyResetTime;
    }

    public void DrawWeeklyResetBar()
    {
        // Calculate the time remaining until the next weekly reset
        var now = DateTime.UtcNow;


        var nextWeeklyResetTime = NextWeeklyResetTime();
        var timeUntilWeeklyReset = nextWeeklyResetTime - now;

        var weeklyProgress = 1 - (timeUntilWeeklyReset.TotalSeconds / (7 * 24 * 60 * 60));

        var dayText = "";
        if (timeUntilWeeklyReset.TotalDays > 1)
        {
            dayText = timeUntilWeeklyReset.TotalDays > 2
                ? $"{timeUntilWeeklyReset.Days.ToString()} days, "
                : $"{timeUntilWeeklyReset.Days.ToString()} day, ";
        }

        var text = $"Weekly Reset: {dayText}{timeUntilWeeklyReset.ToString("hh':'mm':'ss")}";
        UiHelpers.BufferingBar(text, configuration.WeeklyTrackerSettings.BackgroundColor,
            configuration.WeeklyTrackerSettings.ForegroundColor, configuration.WeeklyTrackerSettings.BorderColor,
            configuration.WeeklyTrackerSettings.TextColor,
            configuration.WeeklyTrackerSettings.Width, configuration.WeeklyTrackerSettings.Height,
            configuration.WeeklyTrackerSettings.BorderThickness, (float)weeklyProgress);
    }

    private void DrawTrackerSettings()
    {
        var shouldSave = false;
        if (ImGui.CollapsingHeader("Weekly Tracker Settings:"))
        {
            shouldSave |= UiHelpers.TrackerSettingEdit(configuration.WeeklyTrackerSettings, "Weekly Tracker");
        }

        if (ImGui.CollapsingHeader("Daily Tracker Settings:"))
        {
            shouldSave |= UiHelpers.TrackerSettingEdit(configuration.DailyTrackerSettings, "Daily Tracker");
        }

        if (shouldSave)
        {
            configuration.Save();
        }
    }
}