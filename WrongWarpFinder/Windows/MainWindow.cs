using System;
using System.Numerics;
using Dalamud.Interface;
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
        if (Plugin.ClientState.LocalPlayer == null) return;

        bool show = Plugin.Configuration.ShowOverlay;
        if (ImGui.Checkbox("Show Overlay", ref show))
        {
            Plugin.Configuration.ShowOverlay = show;
            Plugin.Configuration.Save();
            Plugin.ShowHideOverlay();
        }

        if (ImGui.Button("Add New Cube"))
        {
            Vector3 pos = Plugin.ClientState.LocalPlayer.Position;

            Plugin.CubeToManipulate = -1;
            Plugin.CubesToRender.Add(new Cube(pos, Vector3.One, Vector3.Zero));
        }

        int id = 0;
        for (int i = 0; i < Plugin.CubesToRender.Count; i++)
        {
            Cube cube = Plugin.CubesToRender[i];

            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.HandPointDown))
            {
                Plugin.CubesToRender[i].Position = Plugin.ClientState.LocalPlayer.Position;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Move this position to your position");
            }

            ImGui.SameLine();
            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Ruler))
            {
                Plugin.CubeToManipulate = i;
            }

            ImGui.SameLine();

            id++;
            var ctrl = ImGui.GetIO().KeyCtrl;
            using (ImRaii.Disabled(!ctrl))
            {
                if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Trash))
                {
                    Plugin.CubesToRender.RemoveAt(i);
                    Plugin.CubeToManipulate = -1;
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Ctrl+click to delete");
            }

            id++;
            Vector3 position = cube.Position;
            if (ImGui.DragFloat3($"Position##{id}", ref position, 0.1f))
            {
                Plugin.CubesToRender[i].Position = position;
            }

            id++;
            Vector3 scale = cube.Scale;
            if (ImGui.DragFloat3($"Scale##{id}", ref scale, 0.1f, 0.01f, float.MaxValue))
            {
                Plugin.CubesToRender[i].Scale = scale;
                Plugin.CubesToRender[i].UpdateVerts();
            }

            id++;
            Vector3 rotation = cube.Rotation;
            if (ImGui.DragFloat3($"Rotation##{id}", ref rotation, 0.1f))
            {
                Plugin.CubesToRender[i].Rotation = rotation;
            }
        }

        if (ImGui.Button("Add Current Position"))
        {
            Plugin.PositionsToRender.Add(Plugin.ClientState.LocalPlayer.Position);
        }

        for (int i = 0; i < Plugin.PositionsToRender.Count; i++)
        {
            Vector3 posToRender = Plugin.PositionsToRender[i];

            id++;
            if (ImGui.DragFloat3($"##Position{id}", ref posToRender, 0.01f))
            {
                Plugin.PositionsToRender[i] = posToRender;
            }

            ImGui.SameLine();
            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.HandPointDown))
            {
                Plugin.PositionsToRender[i] = Plugin.ClientState.LocalPlayer.Position;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Move this position to your position");
            }

            ImGui.SameLine();

            id++;
            var ctrl = ImGui.GetIO().KeyCtrl;
            using (ImRaii.Disabled(!ctrl))
            {
                if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Trash))
                {
                    Plugin.PositionsToRender.RemoveAt(i);
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Ctrl+click to delete");
            }
        }
    }
}
