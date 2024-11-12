using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace KangasTweaks.WeatherModule;

public class WeatherManager
{
    
    public List<TerritoryType> territoryTypesFiltered;

    public readonly Dictionary<uint, TerritoryType> territoryTypes;
    
    public readonly Dictionary<uint, Weather> weathers;

    public readonly Dictionary<uint, WeatherRate> weatherRates;

    public readonly IDataManager dataManager;
    private readonly IPluginLog pluginLog;

    public WeatherManager(IDataManager dataManager, IPluginLog pluginLog)
    {
        this.pluginLog = pluginLog;
        weathers = dataManager.Excel.GetSheet<Weather>().ToDictionary(x => x.RowId, x => x);
        this.territoryTypes = dataManager.Excel.GetSheet<TerritoryType>().ToDictionary(x => x.RowId, x => x);
        territoryTypesFiltered = dataManager.Excel.GetSheet<TerritoryType>().Where(x =>
                !x.IsPvpZone
                && x.PlaceName.ValueNullable != null
                && !string.IsNullOrWhiteSpace(x.PlaceName.Value.Name.ToString()))
            .DistinctBy(x => x.PlaceName.Value.Name.ToString())
            .OrderBy(x => x.PlaceName.Value.Name.ToString())
            .ToList();
        weatherRates = dataManager.Excel.GetSheet<WeatherRate>().ToDictionary(x => x.RowId, x => x);
    }
    
    public IEnumerable<(uint, uint)> GetWeatherRatesFromZoneId(uint zoneId)
    {
        var territory = territoryTypes[zoneId];
        return GetWeatherRatesFromTerritory(territory);
    }
    
    
    public IEnumerable<(uint, uint)> GetWeatherRatesFromTerritory(TerritoryType territoryType)
    {
        pluginLog.Information(territoryType.WeatherRate.ToString());
        var weatherRate = weatherRates[territoryType.WeatherRate];
        var dc = new List<(uint, uint)>();
        var counter = 0;
        // Wtf is this, why are you like this, why did you do this
        var rates = weatherRate.Rate.Where(x => x > 0).ToArray();
        var p = 0;
        for (int i = 0; i < rates.Length; i++)
        {
            if (rates[i] <= 0) continue;
            var rate = rates[i];
            var weather = weatherRate.Weather[p];
            counter += rate;
            p += 1;
            dc.Add(((uint)counter, (uint)weather.Value.RowId));
        }
        return dc;
    }
}