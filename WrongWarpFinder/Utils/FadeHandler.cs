using System;
using Dalamud.Game.NativeWrapper;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace WrongWarpFinder.Utils;

public class FadeHandler : IDisposable
{
    private Plugin Plugin;
    
    public FadeHandler(Plugin plugin)
    {
        Plugin = plugin;
        if (!Plugin.Configuration.HideFade) return;
        Enable();
    }

    public void Enable()
    {
        Plugin.Framework.Update += Update;
    }

    public void Dispose()
    {
        Plugin.Framework.Update -= Update;
        UpdateFade(true);
    }

    private void Update(IFramework framework)
    {
        try
        {
            if (!Plugin.Configuration.HideFade) return;
            UpdateFade();
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }
    }

    // Old Airiel plogon code
    private unsafe void UpdateFade(bool reset = false)
    { 
        var fade = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("FadeMiddle").Address;
        if ((IntPtr)fade == IntPtr.Zero) return;

        if (reset)
        {
            fade->IsVisible = true;
            fade->Alpha = byte.MaxValue;
        }
        else
        {
            fade->IsVisible = false;
            fade->Alpha = 0;
        }
    }
}
