using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Interface.Internal;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiScene;

namespace KangasTweaks;

public class ImageStore
{
    private Dictionary<int, IDalamudTextureWrap> imageStoreBase;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly ITextureProvider dataManager;

    public ImageStore(DalamudPluginInterface dalamudPluginInterface, ITextureProvider dataManager)
    {
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dataManager = dataManager;
        imageStoreBase = new Dictionary<int, IDalamudTextureWrap>();
    }

    public IDalamudTextureWrap GetIcon(int id)
    {
        if (imageStoreBase.TryGetValue(id, out var val))
        {
            return val;
        }

        var texIcon = dataManager.GetIcon((uint)id);
        
        imageStoreBase[id] = texIcon;
        return texIcon;
    }
}