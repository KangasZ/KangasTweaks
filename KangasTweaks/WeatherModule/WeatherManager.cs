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
    
    /** Note for my own sanity:
     * {
     *    weatherRate: {
     *       rate: [
     *          10, 25, 10, 25, 30
     *       ],
     *       weather: [
     *           { fair skies },
     *           { heat waves },
     *           { etc },
     *           { etc },
     *           { etc }
     *      ]
     *    }
     * }
     */
    
    public IEnumerable<(uint, uint)> GetWeatherRatesFromTerritory(TerritoryType territoryType)
    {
        var weatherRate = weatherRates[territoryType.WeatherRate];
        var dc = new List<(uint, uint)>();
        var counter = 0;
        // Wtf is this, why are you like this, why did you do this, who hurt you, just combine the rates and the weather objects grrr grr
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