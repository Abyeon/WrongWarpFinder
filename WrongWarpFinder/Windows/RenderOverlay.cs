using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Pictomancy;
using WrongWarpFinder.Shapes;
using WrongWarpFinder.Utils;
using WrongWarpFinder.Utils.Interop;

namespace WrongWarpFinder.Windows
{
    public class RenderOverlay : Window, IDisposable
    {
        private Plugin Plugin { get; }

        private ImGuiIOPtr io;

        public RenderOverlay(Plugin plugin) : base ("Render Overlay")
        {
            Flags = ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoDocking
                | ImGuiWindowFlags.NoNavFocus
                | ImGuiWindowFlags.NoNavInputs
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoInputs;

            this.Plugin = plugin;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private unsafe void DrawDebug(PctDrawList drawList)
        {
            var info = WarpInfo.Instance();
            var lastHit = info->ZoneLineLastHit;
            var warp = info->WarpPos;
            drawList.AddPositionArrow(lastHit, "Zone Line Hit", 0xFF00FFFF);
            drawList.AddPositionArrow(warp, "Warp Position", 0xFFFF0000);

            foreach (var cube in Plugin.BrokenExitRanges)
            {
                drawList.AddText(cube.Transform.Position + new Vector3(0, cube.Transform.Scale.Y + 0.5f, 0), 0xFFFFFFFF, "Exit Range", 1);
                drawList.AddCubeFilled(cube, 0x550000FF);
            }
            //drawList.AddPathLine(, Plugin.LineToDraw[1], 0xFF00FFFF);
        }
        
        public override unsafe void Draw()
        {
            ImGuiHelpers.SetWindowPosRelativeMainViewport("Render Overlay", new Vector2(0, 0));

            ImDrawListPtr imDrawList = ImGui.GetWindowDrawList();
            
            io = ImGui.GetIO();
            ImGui.SetWindowSize(io.DisplaySize);

            using var drawList = PictoService.Draw();
            if (drawList == null) return;
            DrawDebug(drawList);
                
            for (int i = 0; i < Plugin.Configuration.CubesToRender.Count; i++)
            {
                Cube cube = Plugin.Configuration.CubesToRender[i];
                drawList.AddCubeFilled(cube, 0x55FF2222);

                if (i == Plugin.CubeToManipulate)
                {
                    // Make a copy of the cube
                    Cube copy = Plugin.Configuration.CubesToRender[i];

                    // Do manipulation
                    Transform transform = cube.Transform;

                    if (DrawExtensions.Manipulate(ref transform,0.25f, "WrongWarpFinderGizmo"))
                    {
                        cube.Transform = transform;
                    }

                    // If the cube was manipulated, save the config again.
                    if (copy != Plugin.Configuration.CubesToRender[i])
                    {
                        Plugin.Configuration.Save();
                    }
                }
            }
                
            foreach (Vector3 pos in Plugin.Configuration.PositionsToRender)
            {
                drawList.AddPositionArrow(pos, pos.ToString(), 0xFF00FF00);
            }
        }
    }
}
