using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;
using WrongWarpFinder.Shapes;

namespace WrongWarpFinder;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool Announcements { get; set; } = true;
    public bool ShowOverlay { get; set; } = true;
    public bool HideFade { get; set; } = false;
    public bool ShowDebugInfo { get; set; } = false;
    public bool ShowOnlyBrokenRanges { get; set; } = false;

    public List<Vector3> PositionsToRender { get; init; } = new List<Vector3>();
    public List<Cube> CubesToRender { get; init; } = new List<Cube>();

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
