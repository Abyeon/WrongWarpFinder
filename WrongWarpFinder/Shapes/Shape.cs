using System.Numerics;
namespace WrongWarpFinder.Shapes;

public abstract class Shape(Vector3 position, Vector3 scale, Vector3 rotation)
{
    public Transform Transform { get; set; } = new(position, scale, rotation);
    protected abstract Vector3[] Vertices { get; }
    
    public Vector3[] GetTransformedVerts()
    {
        Vector3[] transformed = new Vector3[Vertices.Length];
        Matrix4x4 matrix = Transform.GetTransformation();
        
        // Transform vertices
        for (int i = 0; i < Vertices.Length; i++)
        {
            transformed[i] = Vector3.Transform(Vertices[i], matrix);
        }
        
        return transformed;
    }
    
    public abstract bool PointInside(Vector3 point);
}
