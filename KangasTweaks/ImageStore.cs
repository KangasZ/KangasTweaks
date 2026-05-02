using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace KangasTweaks;

public class ImageStore
{
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly ITextureProvider dataManager;

    public ImageStore(IDalamudPluginInterface dalamudPluginInterface, ITextureProvider dataManager)
    {
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dataManager = dataManager;
    }

    public IDalamudTextureWrap GetIcon(int id)
    {
        var texIcon = dataManager.GetFromGameIcon(id);
        return texIcon.GetWrapOrEmpty();
    }
}