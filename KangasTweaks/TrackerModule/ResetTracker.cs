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
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawConfigUi;
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
            // Calculate the time remaining until the next weekly reset
            var now = DateTime.UtcNow;
            var daysUntilNextTuesday = ((int)DayOfWeek.Tuesday - (int)now.DayOfWeek + 7) % 7;
            var nextWeeklyResetTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc).AddDays(daysUntilNextTuesday);
            var timeUntilWeeklyReset = nextWeeklyResetTime - now;

            // Calculate the time remaining until the next daily reset
            var nextDailyResetTime = now.Date.AddHours(15);
            if (now.Hour >= 15)
            {
                nextDailyResetTime = nextDailyResetTime.AddDays(1);
            }
            var timeUntilDailyReset = nextDailyResetTime - now;

            // Calculate the progress as a percentage of the week and day
            var weeklyProgress = 1- (timeUntilWeeklyReset.TotalSeconds / (7*24*60*60));
            var dailyProgress = 1- (timeUntilDailyReset.TotalSeconds / (24*60*60));

            ImGui.Text("Weekly Reset Time: " + nextWeeklyResetTime.ToLocalTime());
            ImGui.Text("Time until weekly reset: " + timeUntilWeeklyReset.ToString(@"dd\:hh\:mm\:ss"));
            ImGui.Text("Daily Reset Time: " + nextDailyResetTime.ToLocalTime());
            ImGui.Text("Time until daily reset: " + timeUntilDailyReset.ToString(@"hh\:mm\:ss"));

            var dayText = "";
            if (timeUntilWeeklyReset.TotalDays > 1)
            {
                dayText = timeUntilWeeklyReset.TotalDays > 2 ? $"{timeUntilWeeklyReset.TotalDays} days" : $"{timeUntilWeeklyReset.TotalDays} day";
            }
            var text = $"Weekly Reset: {dayText} {timeUntilWeeklyReset.ToString("hh':'mm':'ss")}";
            
            // Update the progress bars with the new progress values
            UiHelpers.BufferingBar(text, configuration.WeeklyTrackerSettings.BackgroundColor,
                configuration.WeeklyTrackerSettings.ForegroundColor, configuration.WeeklyTrackerSettings.BorderColor, configuration.WeeklyTrackerSettings.TextColor,
                configuration.WeeklyTrackerSettings.Width, configuration.WeeklyTrackerSettings.Height, configuration.WeeklyTrackerSettings.BorderThickness,  (float)weeklyProgress);
            UiHelpers.BufferingBar($"Daily Reset: {timeUntilDailyReset.ToString("hh':'mm':'ss")}", configuration.DailyTrackerSettings.BackgroundColor,
                configuration.DailyTrackerSettings.ForegroundColor, configuration.DailyTrackerSettings.BorderColor, configuration.DailyTrackerSettings.TextColor,
                configuration.DailyTrackerSettings.Width, configuration.DailyTrackerSettings.Height, configuration.DailyTrackerSettings.BorderThickness, (float)dailyProgress);
            // Draw the tracker settings
            DrawTrackerSettings();
        }

        ImGui.End();
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