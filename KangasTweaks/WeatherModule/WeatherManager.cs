using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace KangasTweaks.WeatherModule;

public class WeatherManager
{
    
    public List<TerritoryType> territoryTypesFiltered;

    public readonly Dictionary<uint, TerritoryType> territoryTypes;
    
    public readonly Dictionary<uint, Weather> weathers;

    public readonly Dictionary<uint, WeatherRate> weatherRates;

    public readonly IDataManager dataManager;
    
    public WeatherManager(IDataManager dataManager)
    {
        weathers = dataManager.Excel.GetSheet<Weather>().ToDictionary(x => x.RowId, x => x);
        this.territoryTypes = dataManager.Excel.GetSheet<TerritoryType>().ToDictionary(x => x.RowId, x => x);
        territoryTypesFiltered = dataManager.Excel.GetSheet<TerritoryType>().Where(x =>
                !x.IsPvpZone
                && x.PlaceName.Value != null
                && !string.IsNullOrWhiteSpace(x.PlaceName.Value.Name.RawString))
            .DistinctBy(x => x.PlaceName.Value.Name.RawString)
            .OrderBy(x => x.PlaceName.Value.Name.RawString)
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
        var weatherRate = weatherRates[territoryType.WeatherRate];
        var dc = new List<(uint, uint)>();
        var counter = 0;
        foreach (var data in weatherRate.UnkData0.Where(x => x.Rate > 0))
        {
            var rate = data.Rate;
            var weather = data.Weather;
            counter += rate;
            dc.Add(((uint)counter, (uint)weather));
        }

        return dc;
    }
}