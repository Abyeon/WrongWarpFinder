using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;
using Pictomancy;
using WrongWarpFinder.Shapes;
using WrongWarpFinder.Utils;

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

        public override unsafe void Draw()
        {
            ImGuiHelpers.SetWindowPosRelativeMainViewport("Render Overlay", new Vector2(0, 0));

            ImDrawListPtr imDrawList = ImGui.GetWindowDrawList();
            //DrawHelper draw = new DrawHelper(imDrawList);
            
            io = ImGui.GetIO();
            ImGui.SetWindowSize(io.DisplaySize);

            using (var drawList = PictoService.Draw())
            {
                if (drawList == null) return;
                
                for (int i = 0; i < Plugin.Configuration.CubesToRender.Count; i++)
                {
                    Cube cube = Plugin.Configuration.CubesToRender[i];
                    drawList.AddCubeFilled(cube, 0x55FF2222);

                    if (i == Plugin.CubeToManipulate)
                    {
                        // Make a copy of the cube
                        Cube copy = Plugin.Configuration.CubesToRender[i];

                        // Do manipulation
                        Vector3 pos = cube.Transform.Position;
                        Vector3 scale = cube.Transform.Scale;
                        Vector3 rotation = new Vector3(cube.Transform.Rotation.Y, cube.Transform.Rotation.X, cube.Transform.Rotation.Z) * (float)(180/Math.PI);

                        if (DrawExtensions.Manipulate(ref pos, ref rotation, ref scale,0.25f, "WrongWarpFinderGizmo"))
                        {
                            cube.Transform.Position = pos;
                            cube.Transform.Rotation = new Vector3(rotation.Y, rotation.X, rotation.Z) * (float)(Math.PI/180);
                            cube.Transform.Scale = scale;
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
                    // Draw an arrow pointing to the position
                    drawList.AddPathLine(pos, pos + new Vector3(0, 2f, 0), 0xFF00FF00);
                    drawList.AddPathLine(pos, pos + new Vector3(0.5f, 0.5f, 0), 0xFF00FF00);
                    drawList.AddPathLine(pos, pos + new Vector3(-0.5f, 0.5f, 0), 0xFF00FF00);

                    // Draw the text saying the position
                    drawList.AddText(pos + new Vector3(0, 3f, 0), 0xFFFFFFFF, pos.ToString(), 1f);
                }
            }
        }
    }
}
