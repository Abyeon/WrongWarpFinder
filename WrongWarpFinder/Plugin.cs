using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using WrongWarpFinder.Windows;
using System.Numerics;
using System.Collections.Generic;

namespace WrongWarpFinder;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;

    private const string CommandName = "/wwfinder";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("WrongWarpFinder");
    private MainWindow MainWindow { get; init; }
    private RenderOverlay RenderOverlay { get; init; }

    public List<Vector3> PositionsToRender { get; init; } = new List<Vector3>();
    public List<Cube> CubesToRender { get; init; } = new List<Cube>();
    public int CubeToManipulate { get; set; } = -1;
    
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);
        RenderOverlay = new RenderOverlay(this);

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(RenderOverlay);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [WrongWarpFinder] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
        ShowHideOverlay();
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

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleMainUI() => MainWindow.Toggle();
}
