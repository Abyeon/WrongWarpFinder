using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using WrongWarpFinder.Windows;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Pictomancy;
using WrongWarpFinder.Shapes;
using WrongWarpFinder.Utils;
using WrongWarpFinder.Utils.Interop;

namespace WrongWarpFinder;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private const string CommandName = "/wwfinder";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("WrongWarpFinder");
    private MainWindow MainWindow { get; init; }
    private RenderOverlay RenderOverlay { get; init; }
    public WarpHook WarpHook { get; init; }
    public int CubeToManipulate { get; set; } = -1;

    public List<Cube> BrokenExitRanges { get; set; } = [];

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        
        PictoService.Initialize(PluginInterface);

        MainWindow = new MainWindow(this);
        RenderOverlay = new RenderOverlay(this);

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(RenderOverlay);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the wrong warp finder menu"
        });

        WarpHook = new WarpHook(this);

        PluginInterface.UiBuilder.Draw += DrawUI;
        Framework.Update += Update;
        ClientState.ZoneInit += OnZoneInit;
        
        UpdateBrokenExitRanges(ClientState.TerritoryType);

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        ShowHideOverlay();
    }

    public void UpdateBrokenExitRanges(uint id)
    {
        BrokenExitRanges = [];
        var broken = BrokenRangeFinder.ScanTerritory(id, showAll: !Configuration.ShowOnlyBrokenRanges);

        if (broken.Count == 0) return;
        
        if (Configuration.Announcements) ChatGui.Print("Detected broken exit range!");
        float radToDeg = 180f / (float)Math.PI;
        foreach (var obj in broken)
        {
            var cube = new Cube(new Vector3(obj.Transform.Translation.X, obj.Transform.Translation.Y - obj.Transform.Scale.Y, obj.Transform.Translation.Z), 
                                new Vector3(obj.Transform.Scale.X,  obj.Transform.Scale.Y * 2, obj.Transform.Scale.Z), 
                                new Vector3(obj.Transform.Rotation.X * radToDeg,  obj.Transform.Rotation.Y * radToDeg, obj.Transform.Rotation.Z * radToDeg));
            BrokenExitRanges.Add(cube);
        }
    }

    private void OnZoneInit(ZoneInitEventArgs zone)
    {
        UpdateBrokenExitRanges(zone.TerritoryType.RowId);
    }

    public Vector3? LastWarpPos;
    private unsafe void CheckWarpPos()
    {
        var info = WarpInfo.Instance();
        if (info is null) return; // Shouldn't be, but in case the sig becomes bad >.>

        try
        {
            if (LastWarpPos != null)
            {
                var pos = info->WarpPos;
                if (LastWarpPos != pos)
                {
                    // Warp pos updated!
                    if (Configuration.Announcements) ChatGui.Print($"Warp pos updated! {pos}");
                    LastWarpPos = pos;
                }

                return;
            }

            LastWarpPos = info->WarpPos;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    private void Update(IFramework framework)
    {
        if (ClientState.LocalPlayer == null) return;
        CheckWarpPos();
    }

    public void ShowHideOverlay()
    {
        if (Configuration.ShowOverlay)
        {
            RenderOverlay.IsOpen = true;
        } else
        {
            RenderOverlay.IsOpen = false;
        }
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        WarpHook.Dispose();

        CommandManager.RemoveHandler(CommandName);
        Framework.Update -= Update;
        ClientState.ZoneInit -= OnZoneInit;
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleMainUI() => MainWindow.Toggle();
}
