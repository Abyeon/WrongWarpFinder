using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using WrongWarpFinder.Shapes;
using WrongWarpFinder.Utils;
using WrongWarpFinder.Utils.Interop;

namespace WrongWarpFinder.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("WrongWarp Finder##RacingwayIsAwesome")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public void Dispose() { }

    public unsafe void SetFlagMarkerPosition(Vector3 position, string title)
    {
        try
        {
            var agent = AgentMap.Instance();
            uint territory = agent->CurrentTerritoryId;
            uint map = agent->CurrentMapId;

            agent->SetFlagMapMarker(territory, map, position);
            agent->OpenMap(map, territory, title);
        } catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }
    }

    private unsafe void DrawWarpPositions()
    {
        try
        {
            if (!Plugin.Configuration.ShowDebugInfo) return;
            var info = WarpInfo.Instance();
            if (info is null) return;
            
            var pos = info->WarpPos;
            ImGui.Text($"Warping to: {pos}");
            
            var warpingState = info->WarpingState;
            ImGui.Text($"Warping State: {warpingState}");
            
            //ImGui.Text($"BgName: {*info->BgName}");
            //ImGui.Text($"Unk184: {info->Unk184}");
            ImGui.Text($"Unk188: {info->Unk188}");
            ImGui.Text($"TerritoryID: {info->TerritoryId}");
            
            ReadOnlySeString territory = "No zone fetched";
            if (info->TerritoryId >= 1)
                territory = Plugin.DataManager.GetExcelSheet<TerritoryType>().GetRow(info->TerritoryId).PlaceName.Value.Name;
            
            ImGui.Text($"TerritoryName: {territory}");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }
    }

    private unsafe void DrawWarpEditor(int id)
    {
        using var _ = ImRaii.Disabled();
        
        var info = WarpInfo.Instance();
        if (info is null) return;
        if (Plugin.ClientState.LocalPlayer == null) return;

        try
        {
            Vector3 pos = info->WarpPos;
            if (ImGui.DragFloat3("Warp Position", ref pos, 0.1f)) { }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex.ToString());
        }
    }

    public override void Draw()
    {
        if (Plugin.ClientState.LocalPlayer == null) return;
        var ctrl = ImGui.GetIO().KeyCtrl;
        
        bool show = Plugin.Configuration.ShowOverlay;
        if (ImGui.Checkbox("Show Overlay", ref show))
        {
            Plugin.Configuration.ShowOverlay = show;
            Plugin.Configuration.Save();
            Plugin.ShowHideOverlay();
        }
        
        bool announce = Plugin.Configuration.Announcements;
        if (ImGui.Checkbox("Announce changes", ref announce))
        {
            Plugin.Configuration.Announcements = announce;
            Plugin.Configuration.Save();
        }
        
        bool debug = Plugin.Configuration.ShowDebugInfo;
        if (ImGui.Checkbox("Debug Info", ref debug))
        {
            Plugin.Configuration.ShowDebugInfo = debug;
            Plugin.Configuration.Save();
        }
        ImGuiComponents.HelpMarker("This is mostly for dev purposes, but you may find the info appealing.");
        
        DrawWarpPositions();
        
        int id = 0;
        DrawWarpEditor(id);

        if (Plugin.CubeToManipulate != -1)
        {
            ImGui.SameLine();
            if (ImGui.Button("Stop Editing"))
            {
                Plugin.CubeToManipulate = -1;
            }
        }
        
        if (ImGui.Button("Add New Cube"))
        {
            Vector3 pos = Plugin.ClientState.LocalPlayer.Position;

            Plugin.CubeToManipulate = -1;
            Plugin.Configuration.CubesToRender.Add(new Cube(pos, Vector3.One, Vector3.Zero));
            Plugin.Configuration.Save();
        }

        using (ImRaii.Disabled(!ctrl))
        {
            if (Plugin.Configuration.CubesToRender.Count > 0)
            {
                ImGui.SameLine();
                if (ImGui.Button("Clear Cubes"))
                {
                    Plugin.Configuration.CubesToRender.Clear();
                    Plugin.Configuration.Save();
                }
            }
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip("Ctrl+click to activate");
        }

        for (int i = 0; i < Plugin.Configuration.CubesToRender.Count; i++)
        {
            Cube cube = Plugin.Configuration.CubesToRender[i];

            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.HandPointDown))
            {
                Plugin.Configuration.CubesToRender[i].Transform.Position = Plugin.ClientState.LocalPlayer.Position;
                Plugin.Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Move this cube to your position");
            }

            ImGui.SameLine();
            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Ruler))
            {
                Plugin.CubeToManipulate = i;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Edit with the gizmo");
            }

            ImGui.SameLine();
            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Flag))
            {
                SetFlagMarkerPosition(cube.Transform.Position, "Wrong Warp?");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set a flag for the marker's position.");
            }

            ImGui.SameLine();

            id++;
            using (ImRaii.Disabled(!ctrl))
            {
                if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Trash))
                {
                    Plugin.Configuration.CubesToRender.RemoveAt(i);
                    Plugin.CubeToManipulate = -1;
                    Plugin.Configuration.Save();
                }
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Ctrl+click to delete");
            }

            id++;
            Vector3 position = cube.Transform.Position;
            if (ImGui.DragFloat3($"Position##{id}", ref position, 0.1f))
            {
                Plugin.Configuration.CubesToRender[i].Transform.Position = position;
                Plugin.Configuration.Save();
            }

            id++;
            Vector3 scale = cube.Transform.Scale;
            if (ImGui.DragFloat3($"Scale##{id}", ref scale, 0.1f, 0.01f, float.MaxValue))
            {
                Plugin.Configuration.CubesToRender[i].Transform.Scale = scale;
                Plugin.Configuration.Save();
            }

            id++;
            Vector3 rotation = cube.Transform.Rotation;
            if (ImGui.DragFloat3($"Rotation##{id}", ref rotation, 0.1f))
            {
                Plugin.Configuration.CubesToRender[i].Transform.Rotation = rotation;
                Plugin.Configuration.Save();
            }
        }

        if (ImGui.Button("Add Current Position"))
        {
            Plugin.Configuration.PositionsToRender.Add(Plugin.ClientState.LocalPlayer.Position);
            Plugin.Configuration.Save();
        }

        using (ImRaii.Disabled(!ctrl))
        {
            if (Plugin.Configuration.PositionsToRender.Count > 0)
            {
                ImGui.SameLine();
                if (ImGui.Button("Clear Positions"))
                {
                    Plugin.Configuration.PositionsToRender.Clear();
                    Plugin.Configuration.Save();
                }
            }
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip("Ctrl+click to activate");
        }

        for (int i = 0; i < Plugin.Configuration.PositionsToRender.Count; i++)
        {
            Vector3 posToRender = Plugin.Configuration.PositionsToRender[i];

            id++;
            if (ImGui.DragFloat3($"##Position{id}", ref posToRender, 0.01f))
            {
                Plugin.Configuration.PositionsToRender[i] = posToRender;
                Plugin.Configuration.Save();
            }

            ImGui.SameLine();
            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.HandPointDown))
            {
                Plugin.Configuration.PositionsToRender[i] = Plugin.ClientState.LocalPlayer.Position;
                Plugin.Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Move this position to your position");
            }

            ImGui.SameLine();
            id++;
            if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Flag))
            {
                SetFlagMarkerPosition(posToRender, "Wrong Warp?");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set a flag for the marker's position.");
            }

            ImGui.SameLine();

            id++;
            using (ImRaii.Disabled(!ctrl))
            {
                if (ImGuiComponents.IconButton(id, FontAwesomeIcon.Trash))
                {
                    Plugin.Configuration.PositionsToRender.RemoveAt(i);
                    Plugin.Configuration.Save();
                }
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Ctrl+click to delete");
            }
        }
    }
}
