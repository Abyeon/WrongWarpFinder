using System.Numerics;

namespace WrongWarpFinder.Shapes;

public class Transform
{
    private Vector3 position;
    private Vector3 scale;
    private Vector3 rotation;
    
    public Matrix4x4 TransformMatrix;
    private Matrix4x4 inverseTransformMatrix;
    
    public Transform(Vector3 position, Vector3 scale, Vector3 rotation)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;

        UpdateTransformation();
    }
    
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            UpdateTransformation();
        }
    }

    public Vector3 Scale
    {
        get => scale;
        set
        {
            scale = value;
            UpdateTransformation();
        }
    }

    public Vector3 Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
            UpdateTransformation();
        }
    }

    public Matrix4x4 GetTransformation()
    {
        return TransformMatrix;
    }

    public Matrix4x4 GetInverseTransformation()
    {
        return inverseTransformMatrix;
    }

    private void UpdateTransformation()
    {
        Matrix4x4 s = Matrix4x4.CreateScale(scale);
        Matrix4x4 r = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
        Matrix4x4 t = Matrix4x4.CreateTranslation(position);
        TransformMatrix = s * r * t;
        
        Matrix4x4.Invert(TransformMatrix, out inverseTransformMatrix);
    }
}
