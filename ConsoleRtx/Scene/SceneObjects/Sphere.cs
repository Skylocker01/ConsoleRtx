using System.Numerics;

namespace ConsoleRtx.Scene.SceneObjects;

public class Sphere : ISceneObject
{
    private Vector3 _coordinates;
    private int _radius;
    public Sphere(Vector3 coordinates, int radius)
    {
        _coordinates = coordinates;
        _radius = radius;
    }

    public bool CalculateIntersection(Vector3 firstPoint, Vector3 secondPoint)
    {
        var x0 = firstPoint.X;
        var x1 = secondPoint.X;
        var y0 = firstPoint.Y;
        var y1 = secondPoint.Y;
        var z0 = firstPoint.Z;
        var z1 = secondPoint.Z;

        var xc = _coordinates.X;
        var yc = _coordinates.Y;
        var zc = _coordinates.Z;

        var a = MathF.Pow((x0 - xc), 2) + MathF.Pow((y0 - yc), 2) + MathF.Pow((z0 - zc), 2) - MathF.Pow(_radius, 2);
        var c = MathF.Pow((x0 - x1), 2) + MathF.Pow((y0 - y1), 2) + MathF.Pow((z0 - z1), 2);
        var b = MathF.Pow((x1 - xc), 2) + MathF.Pow((y1 - yc), 2) + MathF.Pow((z1 - zc), 2) - a - c - MathF.Pow(_radius, 2);

        var d = MathF.Pow(b, 2) - 4 * a * c;

        if (d < 0)
            return false;

        return true;
    }
}