using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Utility;

using KangasTweaks.Constants;

namespace KangasTweaks;

public static class UiHelpers
{
    public static bool DrawCheckbox(string label, ref bool boxValue, string? tooltipText = null)
    {
        var retStatement = false;
        var tempVar = boxValue;
        if (ImGui.Checkbox(label, ref tempVar))
        {
            boxValue = tempVar;
            retStatement = true;
        }

        if (tooltipText != null)
        {
            LabeledHelpMarker("", tooltipText);
        }

        return retStatement;
    }
    
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
            bgColor, (ImDrawFlags)0);

        windowDrawList.AddRectFilled(cursorScreenPos,
            cursorScreenPos + filledSize,
            fgColor, (ImDrawFlags)0);

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
    
    public static bool Vector4ColorSelector(string label, ref uint configColor, uint? defaultColor = null, ImGuiColorEditFlags flags = ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs)
    {
        var shouldSave = false;
        var tempColor = ImGui.ColorConvertU32ToFloat4(configColor);

        if (ImGui.ColorEdit4(label, ref tempColor, ImGuiColorEditFlags.NoInputs))
        {
            configColor = ImGui.ColorConvertFloat4ToU32(tempColor);
            shouldSave = true;
        }
        if (defaultColor.HasValue)
        {
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-{label}"))
            {
                configColor = defaultColor.Value;
                shouldSave = true;
            }
            ImGui.PopFont();
            UiHelpers.HoverTooltip($"Default: {defaultColor}");
        }
        return shouldSave;
    }
    
    public static bool DrawFloatWithResetSlider(string textDiscription, string id, ref float floatToModify, float min, float max, float defaultFloatValue, string format = "%.2f")
    {
        bool shouldSave = false;
        if (!textDiscription.IsNullOrWhitespace())
        {
            ImGui.Text(textDiscription);
            ImGui.SameLine();
        }
        ImGui.PushItemWidth(150);

        shouldSave |= ImGui.SliderFloat($"##float-slider-{textDiscription}-{id}", ref floatToModify, min, max, format);
        
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-{textDiscription}-{id}"))
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
        shouldSave |= ImGui.Checkbox($"Enabled##{label}-enabled", ref trackerSettings.Enabled);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Foreground Color##{label}-foreground-color", ref trackerSettings.ForegroundColor, defaultColor: ConfigConstants.Blue & ConfigConstants.Opacity60Percent);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Background Color##{label}-background-color", ref trackerSettings.BackgroundColor, defaultColor: ConfigConstants.Black & ConfigConstants.Opacity60Percent);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Border Color##{label}-outline-color", ref trackerSettings.BorderColor, defaultColor: ConfigConstants.Black & ConfigConstants.Opacity60Percent);
        shouldSave |= UiHelpers.Vector4ColorSelector($"Text Color##{label}-text-color", ref trackerSettings.TextColor, defaultColor: ConfigConstants.White);
        shouldSave |= DrawFloatWithResetSlider($"Border Thickness", label, ref trackerSettings.BorderThickness, ConfigConstants.MinBorderThickness, ConfigConstants.MaxBorderThickness, ConfigConstants.DefaultBorderThickness);
        shouldSave |= DrawFloatWithResetSlider($"Width", label, ref trackerSettings.Width, ConfigConstants.MinTrackerWidth, ConfigConstants.MaxTrackerWidth, ConfigConstants.DefaultTrackerWidth);
        shouldSave |= DrawFloatWithResetSlider($"Height", label, ref trackerSettings.Height, ConfigConstants.MinTrackerHeight, ConfigConstants.MaxTrackerHeight, ConfigConstants.DefaultTrackerHeight);
        
        return shouldSave;
    }
}