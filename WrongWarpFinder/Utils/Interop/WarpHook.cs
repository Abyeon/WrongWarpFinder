using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace WrongWarpFinder.Utils.Interop;

public unsafe class WarpHook : IDisposable
{
    public delegate void ZoneWarpInterrupt(long a1, uint a2);
    
    [Signature("E9 ?? ?? ?? ?? C7 01 ?? ?? ?? ?? 41 B0 01", DetourName = nameof(DetourZoneWarpInterrupt))]
    private readonly Hook<ZoneWarpInterrupt>? zoneWarpInterruptHook = null;

    private Plugin plugin;
    
    public WarpHook(Plugin plugin)
    {
        this.plugin = plugin;
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        zoneWarpInterruptHook?.Enable();
    }

    public void Dispose()
    {
        zoneWarpInterruptHook?.Dispose();
    }

    public void DetourZoneWarpInterrupt(long a1, uint a2)
    {
        try
        {
            Plugin.Log.Debug($"Zone Warp Interrupted!\nPointer: {a1}\nHex: {a1:X}");
            
            var warp = (WarpInfo*)a1;
            Plugin.Log.Debug($"Temp Pos: {warp->ZoneLineLastHit}");
            Plugin.Log.Debug($"Coordinates: {warp->WarpPos}");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }

        zoneWarpInterruptHook!.Original.Invoke(a1, a2);
    }
}
