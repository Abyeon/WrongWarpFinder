using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WrongWarpFinder
{
    internal class DrawHelper
    {
        private ImDrawListPtr drawList;

        public DrawHelper(ImDrawListPtr drawListPtr)
        {
            this.drawList = drawListPtr;
        }

        public void DrawText3d(string text, Vector3 position, uint color)
        {
            Vector2 screenPos = new Vector2();

            if (Plugin.GameGui.WorldToScreen(position, out screenPos))
            {
                drawList.AddText(screenPos, color, text);
            }
        }

        public void DrawPoint3d(Vector3 position, uint color, float radius)
        {
            Vector2 screenPos = new Vector2();

            if (Plugin.GameGui.WorldToScreen(position, out screenPos))
            {
                drawList.AddCircleFilled(screenPos, radius, color);
            }
        }

        public void DrawLine3d(Vector3 start, Vector3 end, uint color, float thickness)
        {
            Vector2 screenPos1 = new Vector2();
            Vector2 screenPos2 = new Vector2();

            // Always attempt to draw the line, even if points are off-screen
            // This ensures consistency when zooming in/out
            bool startVisible = Plugin.GameGui.WorldToScreen(start, out screenPos1);
            bool endVisible = Plugin.GameGui.WorldToScreen(end, out screenPos2);

            // Draw the line if at least one end is visible or if both are off-screen but might cross the viewport
            if (startVisible || endVisible)
            {
                drawList.AddLine(screenPos1, screenPos2, color, thickness);
            }
        }

        public void DrawPath3d(Vector3[] path, uint color, float thickness)
        {
            if (path.Length == 0)
                return;

            // Skip drawing if too many points - would cause performance issues
            if (path.Length > 1000)
                return;

            // Draw segments directly instead of using PathLineTo/PathStroke, which can cause
            // visual inconsistencies when the camera moves
            for (int i = 1; i < path.Length; i++)
            {
                DrawLine3d(path[i - 1], path[i], color, thickness);
            }
        }

        public void DrawQuadFilled3d(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, uint color)
        {
            bool onScreen = false;

            Vector3[] worldPos = { p1, p2, p3, p4 };
            Vector2[] screenPos = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                if (Plugin.GameGui.WorldToScreen(worldPos[i], out screenPos[i]))
                {
                    onScreen = true;
                }
            }

            if (onScreen)
            {
                drawList.AddQuadFilled(
                    screenPos[0],
                    screenPos[1],
                    screenPos[2],
                    screenPos[3],
                    color
                );
            }
        }

        public void DrawAABB(Vector3 min, Vector3 max, uint color, float thickness)
        {
            // Define points of the bounding box
            Vector3[] points =
            {
                min,
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z),
                new Vector3(min.X, max.Y, max.Z),
                max,
                new Vector3(max.X, max.Y, min.Z),
            };

            // Define line indices
            int[,] lines =
            {
                { 0, 1 },
                { 1, 2 },
                { 2, 3 },
                { 3, 0 }, // top face
                { 4, 5 },
                { 5, 6 },
                { 6, 7 },
                { 7, 4 }, // bottom face
                { 0, 4 },
                { 1, 5 },
                { 2, 6 },
                {
                    3,
                    7,
                } // Side edges
                ,
            };

            // Go through lines and draw them
            for (int i = 0; i < lines.GetLength(0); i++)
            {
                int start = lines[i, 0];
                int end = lines[i, 1];

                DrawLine3d(points[start], points[end], color, thickness);
            }
        }
    }
}
