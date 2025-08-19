using System;
using System.Numerics;
using Dalamud.Bindings.ImGuizmo;

namespace WrongWarpFinder.Shapes;

public class Transform(Vector3 position, Vector3 scale, Vector3 rotation)
{
    public Vector3 Position = position;
    public Vector3 Scale = scale;
    public Vector3 Rotation = rotation;
    
    public Matrix4x4 GetTransformation()
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        ImGuizmo.RecomposeMatrixFromComponents(ref Position.X, ref Rotation.X, ref Scale.X, ref mat.M11);

        return mat;
        // var s = Matrix4x4.CreateScale(Scale);
        // var rY = Matrix4x4.CreateRotationY(Rotation.Y  * (float)(Math.PI/180));
        // var rX = Matrix4x4.CreateRotationX(Rotation.X  * (float)(Math.PI/180));
        // var rZ = Matrix4x4.CreateRotationZ(Rotation.Z  * (float)(Math.PI/180));
        // var t = Matrix4x4.CreateTranslation(Position);
        //
        // return s * rY * rX * rZ * t;
    }
}
