using System;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using KangasTweaks.Constants;

namespace KangasTweaks;

public static class UiHelpers
{
    public static void DrawTabs(string tabId, params (string label, uint color, Action function)[] tabs)
    {
        ImGui.BeginTabBar($"##{tabId}");
        foreach (var tab in tabs)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, tab.color);
            if (ImGui.BeginTabItem($"{tab.label}##{tabId}"))
            {
                ImGui.PopStyleColor();
                tab.function();
                ImGui.EndTabItem();
            }
            else
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.EndTabBar();
    }
    
    public static void BufferingBar(string label, uint bgColor, uint fgColor, uint borderColor, uint textColor, float xSize, float ySize, float borderThickness, float filledPercent) {
        var tagTextSize = ImGui.CalcTextSize(label);

        var windowDrawList = ImGui.GetWindowDrawList();
        var size = new Vector2(xSize - borderThickness, ySize);
        var filledSize = new Vector2(size.X * filledPercent, size.Y);
        var cursorScreenPos = ImGui.GetCursorScreenPos() + new Vector2(borderThickness, 0);
        windowDrawList.AddRectFilled(cursorScreenPos,
            cursorScreenPos + size,
            bgColor, 0);

        windowDrawList.AddRectFilled(cursorScreenPos,
            cursorScreenPos + filledSize,
            fgColor, 0);

        if (borderThickness > 0)
        {
            windowDrawList.AddRect(cursorScreenPos,
                cursorScreenPos + size,
                borderColor, 0, ImDrawFlags.Closed, borderThickness);
        }

        windowDrawList.AddText(cursorScreenPos + new Vector2(size.X - (tagTextSize.X + borderThickness), size.Y / 2 - tagTextSize.Y / 2), textColor, label);
        ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(0-borderThickness, filledSize.Y + 5));
    }
    
    public static void LabeledHelpMarker(string label, string tooltip)
    {
        ImGuiComponents.HelpMarker(tooltip);
        ImGui.SameLine();
        ImGui.TextUnformatted(label);
        HoverTooltip(tooltip);
    }
    
    public static void HoverTooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }
    
    public static bool Vector4ColorSelector(string label, ref uint configColor, ImGuiColorEditFlags flags = ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs)
    {
        var tempColor = ImGui.ColorConvertU32ToFloat4(configColor);
        if (!ImGui.ColorEdit4(label, ref tempColor, ImGuiColorEditFlags.NoInputs)) return false;
        configColor = ImGui.ColorConvertFloat4ToU32(tempColor);
        return true;
    }
    
    public static bool DrawFloatWithResetSlider(string textDiscription, ref float floatToModify, float min, float max, float defaultFloatValue, string format = "%.2f")
    {
        bool shouldSave = false;
        if (!textDiscription.IsNullOrWhitespace())
        {
            ImGui.Text(textDiscription);
            ImGui.SameLine();
        }
        ImGui.PushItemWidth(150);

        shouldSave |= ImGui.SliderFloat($"##float-slider-{textDiscription}", ref floatToModify, min, max, format);
        
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-{textDiscription}"))
        {
            floatToModify = defaultFloatValue;
            shouldSave = true;
        }
        ImGui.PopFont();
        UiHelpers.HoverTooltip($"Default: {defaultFloatValue.ToString(CultureInfo.InvariantCulture)}");

        return shouldSave;
    }
    
    public static bool TrackerSettingEdit(Configuration.TrackerSettings trackerSettings, string label) {
        var shouldSave = false;
        shouldSave |= UiHelpers.Vector4ColorSelector($"Foreground Color##{label}-foreground-color", ref trackerSettings.ForegroundColor);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Background Color##{label}-background-color", ref trackerSettings.BackgroundColor);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Border Color##{label}-outline-color", ref trackerSettings.BorderColor);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Text Color##{label}-text-color", ref trackerSettings.TextColor);
        shouldSave |= DrawFloatWithResetSlider($"Border Thickness##{label}-border", ref trackerSettings.BorderThickness, ConfigConstants.MinBorderThickness, ConfigConstants.MaxBorderThickness, ConfigConstants.DefaultBorderThickness);
        shouldSave |= DrawFloatWithResetSlider($"Width##{label}-width", ref trackerSettings.Width, ConfigConstants.MinTrackerWidth, ConfigConstants.MaxTrackerWidth, ConfigConstants.DefaultTrackerWidth);
        shouldSave |= DrawFloatWithResetSlider($"Height##{label}-height", ref trackerSettings.Height, ConfigConstants.MinTrackerHeight, ConfigConstants.MaxTrackerHeight, ConfigConstants.DefaultTrackerHeight);
        
        return shouldSave;
    }
}