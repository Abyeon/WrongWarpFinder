using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WrongWarpFinder.Windows
{
    public class RenderOverlay : Window, IDisposable
    {
        private Plugin Plugin { get; }

        private ImGuiIOPtr Io;

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

        public override unsafe void Draw()
        {
            ImGuiHelpers.SetWindowPosRelativeMainViewport("Render Overlay", new Vector2(0, 0));

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            DrawHelper draw = new DrawHelper(drawList);

            Io = ImGui.GetIO();
            ImGui.SetWindowSize(Io.DisplaySize);

            for (int i = 0; i < Plugin.Configuration.CubesToRender.Count; i++)
            {
                Cube cube = Plugin.Configuration.CubesToRender[i];
                draw.DrawCubeFilled(cube, 0x55FF2222, 0.2f);
                draw.DrawCube(cube, 0xFFFF0000, 3f);

                if (i == Plugin.CubeToManipulate)
                {
                    // Make a copy of the cube
                    Cube copy = Plugin.Configuration.CubesToRender[i];

                    // Do manipulation
                    draw.DrawGizmo(ref Plugin.Configuration.CubesToRender[i].Position, ref Plugin.Configuration.CubesToRender[i].Rotation, ref Plugin.Configuration.CubesToRender[i].Scale, "WrongWarpFinderGizmo", 0.25f);

                    // If the cube was manipulated, save the config again.
                    if (copy != Plugin.Configuration.CubesToRender[i])
                    {
                        Plugin.Configuration.Save();
                    }
                }
            }

            foreach (Vector3 pos in Plugin.Configuration.PositionsToRender)
            {
                // Draw an arrow pointing to the position
                draw.DrawLine3d(pos, pos + new Vector3(0, 2f, 0), 0xFF00FF00, 3f);
                draw.DrawLine3d(pos, pos + new Vector3(0.5f, 0.5f, 0), 0xFF00FF00, 3f);
                draw.DrawLine3d(pos, pos + new Vector3(-0.5f, 0.5f, 0), 0xFF00FF00, 3f);

                // Draw the text saying the position
                draw.DrawText3d(pos.ToString(), pos + new Vector3(0, 3f, 0), 0xFFFFFFFF);
            }
        }
    }
}
