using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace KangasTweaks.WeatherModule;

public class WeatherUi
{
    private readonly Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private bool mainWindowVisible = false;
    private readonly IClientState clientState;
    private readonly ImageStore imageStore;
    private readonly WeatherManager weatherManager;

    public WeatherUi(DalamudPluginInterface dalamudPluginInterface,
        Configuration configInterface, IClientState clientState, ImageStore imageStore, WeatherManager weatherManager)
    {
        this.imageStore = imageStore;
        this.weatherManager = weatherManager;
        this.configInterface = configInterface;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.clientState = clientState;
        this.dalamudPluginInterface.UiBuilder.Draw += Draw;
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= Draw;
    }

    public void OpenUi()
    {
        mainWindowVisible = true;
    }

    private void Draw()
    {
        DrawMainWindow();
    }

    private void DrawMainWindow()
    {
        if (!mainWindowVisible)
        {
            return;
        }

        var size = new Vector2(600, 600);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));

        if (ImGui.Begin("Kangas Tweaks Weather##weather-tracker-main", ref mainWindowVisible))
        {
            /*foreach (var territory in kangasTweaksPlugin.territoryTypes)
            {
                var dc = kangasTweaksPlugin.GetWeatherRatesFromZoneId(territory.Key);
                ImGui.Text($"{territory.Value.PlaceName.Value.Name.RawString}");
                foreach (var p in dc)
                {
                    ImGui.Text($"{p.Key}, {p.Value}, {kangasTweaksPlugin.weathers[p.Value].Name.RawString}");
                }
            } */
            List<TerritoryType> territoryList;
            if (configInterface.OnlyShowFavoritedZones)
            {
                territoryList = weatherManager.territoryTypesFiltered.Where(x => configInterface.FavoritedZones.Contains(x.RowId)).ToList();
            }
            else
            {
                territoryList = weatherManager.territoryTypesFiltered;
            }

            string placeholder;

            var territoryFound = weatherManager.territoryTypes.TryGetValue(configInterface.CurrentZoneValIndex, out var currTerritory);
            if (!territoryFound)
            {
                placeholder = "No Selected Item";
            }
            else
            {
                placeholder = currTerritory.PlaceName.Value.Name.RawString;
            }
            ImGui.Text("Hint: Right click on a zone to favorite it.");
            ImGui.PushItemWidth(400);
            if (ImGui.BeginCombo("##Territories-combo-box", placeholder, ImGuiComboFlags.HeightLarge))
            {
                for (int n = 0; n < territoryList.Count; n++)
                {
                    var zone = territoryList[n];
                    var isSelected = configInterface.CurrentZoneValIndex == zone.RowId;
                    var favoritedZonesContains = configInterface.FavoritedZones.Contains(zone.RowId);
                    string selectableText;
                    if (favoritedZonesContains)
                    {
                        selectableText = $"★ {zone.PlaceName.Value.Name.RawString}";
                    }
                    else
                    {
                        selectableText = zone.PlaceName.Value.Name.RawString;
                    }
                    if (ImGui.Selectable(selectableText, isSelected))
                    {
                        configInterface.CurrentZoneValIndex = zone.RowId;
                        configInterface.Save();
                    }

                    if (ImGui.BeginPopupContextItem()) // <-- use last item id as popup id
                    {
                        ImGui.Text($"{territoryList[n].PlaceName.Value.Name.RawString}");
                        if (configInterface.FavoritedZones.Contains(territoryList[n].RowId))
                        {
                            if (ImGui.Button("Remove From Favorites"))
                            {
                                configInterface.FavoritedZones.Remove(territoryList[n].RowId);
                                configInterface.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }
                        else
                        {
                            if (ImGui.Button("Add To Favorites"))
                            {
                                configInterface.FavoritedZones.Add(territoryList[n].RowId);
                                configInterface.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }
                        if (ImGui.Button("Close"))
                            ImGui.CloseCurrentPopup();
                        ImGui.EndPopup();
                    }
                    // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();
            ImGui.SameLine();
            var onlyVisible = configInterface.OnlyShowFavoritedZones;
            if (ImGui.Checkbox("Only Show Favorited Zones", ref onlyVisible))
            {
                configInterface.OnlyShowFavoritedZones = onlyVisible;
                configInterface.Save();
            }
            if (!territoryFound)
            {
                ImGui.Text("Unable to show table for some reason :(");
            }
            else
            {
                DrawWeatherForZone(currTerritory);
            }
        }

        ImGui.End();
    }

    private void DrawWeatherForZone(TerritoryType territoryType)
    {
        ImGui.Text(territoryType.PlaceName.Value.Name.RawString);
        var maxColumns = 3;
        if (ImGui.BeginTable("TrackerTable", maxColumns,
                ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersV |
                ImGuiTableFlags.ScrollY | ImGuiTableFlags.NoSavedSettings))
        {
            ImGui.TableSetupColumn("i"); //, ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("d"); //, ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("k");
            ImGui.TableSetupScrollFreeze(0, 1);

            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
            for (int column = 0; column < maxColumns; column++)
            {
                ImGui.TableSetColumnIndex(column);
                string columnName = ImGui.TableGetColumnName(column);

                ImGui.TableHeader(columnName);
            }

            DrawTimes(territoryType);

            ImGui.EndTable();
        }
    }

    private void DrawTimes(TerritoryType territoryType)
    {
        var weathers = weatherManager.GetWeatherRatesFromTerritory(territoryType);

        ImGui.TableNextRow(ImGuiTableRowFlags.None);
        //        ImGui.Text($"{EorzeaWeather.ToEorzeaTime(DateTime.Now):d MMM yyyy hh:mm tt}");

        var time = EorzeaWeather.LastWeatherIntervalFromIrlTime(
            DateTime.Now.AddSeconds(-10 * EorzeaWeather.EIGHT_EORZEAN_HOURS_IN_IRL_SECONDS));
        var now = DateTime.Now;

        for (var i = 0; i < 80; i++)
        {
            if (i % 3 == 0)
            {
                ImGui.TableNextRow(ImGuiTableRowFlags.None);
                ImGui.TableSetColumnIndex(0);
            }

            var chance = EorzeaWeather.CalculateChance(time);
            var forecast = EorzeaWeather.Forecast(weathers, chance);
            var weather = weatherManager.weathers[forecast];

            var iconId = weather.Icon;
            var iconActual = imageStore.GetIcon(iconId);
            ImGui.Text($"{time:hh:mm tt}");
            ImGui.SameLine();
            ImGui.Image(iconActual.ImGuiHandle, new System.Numerics.Vector2(20, 20));
            ImGui.SameLine();
            ImGui.Text($"{weather.Name.RawString}");
            var nextTime = time.AddSeconds(EorzeaWeather.EIGHT_EORZEAN_HOURS_IN_IRL_SECONDS);
            if (time < now && nextTime > now)
            {
                ImGui.SameLine();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0xff801010);
            }

            ImGui.TableNextColumn();
            time = time.AddSeconds(EorzeaWeather.EIGHT_EORZEAN_HOURS_IN_IRL_SECONDS);
        }
    }
}