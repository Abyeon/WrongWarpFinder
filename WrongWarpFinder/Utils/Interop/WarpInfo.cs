using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace WrongWarpFinder.Utils.Interop;

[StructLayout(LayoutKind.Explicit, Size = 3632)]
public unsafe struct WarpInfo
{
    private static WarpInfo* address = (WarpInfo*)IntPtr.Zero;
    public static WarpInfo* Instance()
    {
        if ((IntPtr)address != IntPtr.Zero) return address;
        
        SigScanner scanner = new SigScanner();
        IntPtr fromSig = scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 89 66 24 4C 89 66 2C 48 8B 0D ?? ?? ?? ??");
        
        if (fromSig != IntPtr.Zero)
        {
            address = (WarpInfo*)fromSig;
            return address;
        }
        
        return null;
    }

    /// <summary>
    /// This is where the player last hit a zone line
    /// </summary>
    [FieldOffset(32)] public Vector3 ZoneLineLastHit;
    
    /// <summary>
    /// The position the player gets warped in the case of a zone warp interrupt
    /// </summary>
    [FieldOffset(48)] public Vector3 WarpPos;
    
    // ---- Zone Pre-fetching information ---
    
    // This might be a counter of some kind, but unsure.
    [FieldOffset(168)] public uint WarpingState; // 0 normally, 1 if near a zone line, 2 if teleporting / returning

    [FieldOffset(176)] public IntPtr ClosestZoneLine;
    [FieldOffset(184)] public byte Unk184;
    [FieldOffset(188)] public uint Unk188;
    
    /// <summary>
    /// The ID of the territory the player is teleporting to.
    /// </summary>
    [FieldOffset(192)] public uint TerritoryId;
}
