using System;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace WrongWarpFinder.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("WrongWarp Finder##RacingwayIsAwesome", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        bool show = Plugin.Configuration.ShowOverlay;
        if (ImGui.Checkbox("Show Overlay", ref show))
        {
            Plugin.Configuration.ShowOverlay = show;
            Plugin.Configuration.Save();
            Plugin.ShowHideOverlay();
        }

        if (Plugin.ClientState.LocalPlayer != null)
        {
            if (ImGui.Button("Add Current Position"))
            {
                Plugin.PositionsToRender.Add(Plugin.ClientState.LocalPlayer.Position);
            }
        }

        int id = 0;
        for (int i = 0; i < Plugin.PositionsToRender.Count; i++)
        {
            Vector3 posToRender = Plugin.PositionsToRender[i];

            id++;
            if (ImGui.DragFloat3($"##Position{id}", ref posToRender, 0.01f))
            {
                Plugin.PositionsToRender[i] = posToRender;
            }

            if (Plugin.ClientState.LocalPlayer != null)
            {
                ImGui.SameLine();
                id++;
                ImGui.PushID(id);
                if (ImGuiComponents.IconButton(id, Dalamud.Interface.FontAwesomeIcon.HandPointDown))
                {
                    Plugin.PositionsToRender[i] = Plugin.ClientState.LocalPlayer.Position;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Move this position to your position");
                }
                ImGui.PopID();
            }

            ImGui.SameLine();
            if (ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
            {
                Plugin.PositionsToRender.Remove(posToRender);
            }
        }
    }
}
