using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager;
using Pictomancy;
using WrongWarpFinder.Shapes;

namespace WrongWarpFinder.Utils;

public static class DrawExtensions
{
    private static unsafe Camera* Camera => &CameraManager.Instance()->GetActiveCamera()->CameraBase.SceneCamera;

    public static unsafe bool Manipulate(ref Transform transform, float snapDistance, string id)
    {
        ImGuizmo.BeginFrame();
        
        var cam = Camera->RenderCamera;
        var view = Camera->ViewMatrix;
        var proj = cam->ProjectionMatrix;

        var far = cam->FarPlane;
        var near = cam->NearPlane;
        var clip = far / (far - near);
        
        proj.M43 = -(clip * near);
        proj.M33 = -((far + near) / (far - near));
        view.M44 = 1.0f;

        ImGuizmo.SetDrawlist();
        ImGuizmo.Enable(true);
        ImGuizmo.SetID((int)ImGui.GetID(id));
        ImGuizmo.SetOrthographic(false);

        Vector2 windowPos = ImGui.GetWindowPos();
        ImGuiIOPtr io = ImGui.GetIO();
        
        ImGuizmo.SetRect(windowPos.X, windowPos.Y, io.DisplaySize.X, io.DisplaySize.Y);

        Matrix4x4 matrix = Matrix4x4.Identity;
        ImGuizmo.RecomposeMatrixFromComponents(ref transform.Position.X, ref transform.Rotation.X, ref transform.Scale.X, ref matrix.M11);
        
        Vector3 snap = Vector3.One * snapDistance;

        ImGuizmoOperation op = ImGuizmoOperation.Translate;

        if (io.KeyCtrl)
        {
            op = ImGuizmoOperation.Scale;
        } else if (io.KeyShift)
        {
            op = ImGuizmoOperation.Rotate;
        }

        SafeManipulate(ref view.M11, ref proj.M11, op, ImGuizmoMode.Local, ref matrix.M11, ref snap.X);
        
        if (ImGuizmo.IsUsing())
        {
            Vector3 pos = new(), rot = new(), scale = new();
            ImGuizmo.DecomposeMatrixToComponents(ref matrix.M11, ref pos.X, ref rot.X, ref scale.X);

            if (op == ImGuizmoOperation.Translate)
            {
                transform.Position = pos;
            }

            if (op == ImGuizmoOperation.Rotate)
            {
                transform.Rotation = rot;
            }

            if (op == ImGuizmoOperation.Scale)
            {
                transform.Scale = scale;
            }
            
            return true;
        }
        
        return false;
    }
    
    private static unsafe bool SafeManipulate(ref float view, ref float proj, ImGuizmoOperation op, ImGuizmoMode mode, ref float matrix, ref float snap)
    {
        fixed (float* nativeView = &view)
        {
            fixed (float* nativeProj = &proj)
            {
                fixed (float* nativeMatrix = &matrix)
                {
                    fixed (float* nativeSnap = &snap)
                    {
                        // Use the ImGuizmo.Manipulate method with proper parameters
                        return ImGuizmo.Manipulate(
                            nativeView,
                            nativeProj,
                            op,
                            mode,
                            nativeMatrix,
                            null,
                            nativeSnap
                        );
                    }
                }
            }
        }
    }
    
    public static void AddCube(this PctDrawList drawList, Cube cube, uint col)
    {
        // Arrays of indices for each face
        int[,] faces =
        {
            { 0, 1, 2, 3 }, // bottom face
            { 7, 6, 5, 4 }  // top face
        };

        // Get transformed vertices of cube
        Vector3[] points = cube.GetTransformedVerts();
        
        drawList.AddQuad(points[faces[0, 0]], points[faces[0, 1]], points[faces[0, 2]], points[faces[0, 3]], col);
        
        drawList.AddPathLine(points[faces[0, 0]], points[faces[1, 3]], col);
        drawList.AddPathLine(points[faces[0, 1]], points[faces[1, 2]], col);
        drawList.AddPathLine(points[faces[0, 2]], points[faces[1, 1]], col);
        drawList.AddPathLine(points[faces[0, 3]], points[faces[1, 0]], col);
        
        drawList.AddQuad(points[faces[1, 0]], points[faces[1, 1]], points[faces[1, 2]], points[faces[1, 3]], col);
    }
    
    public static void AddCubeFilled(this PctDrawList drawList, Cube cube, uint col)
    {
        // Arrays of indices for each face
        int[,] faces =
        {
            { 0, 1, 2, 3 }, // bottom face
            { 7, 6, 5, 4 }, // top face
            { 3, 7, 4, 0 }, // front face
            { 1, 5, 6, 2 }, // back face
            { 0, 4, 5, 1 }, // left face
            { 2, 6, 7, 3 }  // right face
        };

        // Get transformed vertices of cube
        Vector3[] points = cube.GetTransformedVerts();

        for (int i = 0; i < faces.GetLength(0); i++)
        {
            Vector3 p1 = points[faces[i, 0]];
            Vector3 p2 = points[faces[i, 1]];
            Vector3 p3 = points[faces[i, 2]];
            Vector3 p4 = points[faces[i, 3]];

            drawList.AddQuadFilled(p1, p2, p3, p4, col);
        }
    }
    
    public static void AddPathLine(this PctDrawList drawList, Vector3 p1, Vector3 p2, uint col)
    {
        drawList.PathLineTo(p1);
        drawList.PathLineTo(p2);
        drawList.PathStroke(col, PctStrokeFlags.Closed);
    }
}
