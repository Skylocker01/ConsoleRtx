using System.Numerics;
using ConsoleRtx.Models;

namespace ConsoleRtx.Scene.SceneObjects;

public class Sphere : ISceneObject
{
    private Vector3 _position;

    public Vector3 Position
    {
        get => _position;
        private set => _position = value;
    }
    private int _radius;
    //private readonly Vector3 _lightPoint = new Vector3(0, 100, 100);
    
    public Sphere(Vector3 position, int radius)
    {
        _position = position;
        _radius = radius;
    }

    public IntersectionModel? CalculateIntersection(Vector3 firstPoint, Vector3 secondPoint)
    {
        // Использую уравнения в параметрическом виде
        // x(t) = x0*(1-t) + t*x1
        // y(t) = y0*(1-t) + t*y1
        // z(t) = z0*(1-t) + t*z1
        // (x(t) - xc)^2 + (y(t) - yc)^2 + (z(t) - zc)^2 = R^2
        
        var x0 = firstPoint.X;
        var x1 = secondPoint.X;
        var y0 = firstPoint.Y;
        var y1 = secondPoint.Y;
        var z0 = firstPoint.Z;
        var z1 = secondPoint.Z;

        var xc = _position.X;
        var yc = _position.Y;
        var zc = _position.Z;

        var c = MathF.Pow((x0 - xc), 2) + MathF.Pow((y0 - yc), 2) + MathF.Pow((z0 - zc), 2) - MathF.Pow(_radius, 2);
        var a = MathF.Pow((x0 - x1), 2) + MathF.Pow((y0 - y1), 2) + MathF.Pow((z0 - z1), 2);
        var b = MathF.Pow((x1 - xc), 2) + MathF.Pow((y1 - yc), 2) + MathF.Pow((z1 - zc), 2) - a - c - MathF.Pow(_radius, 2);

        var d = MathF.Pow(b, 2) - 4 * a * c;

        if (d < 0)
            return null;

        // Нашли значения параметрa t для точек пересечения со сферой
        var res1 = (-b + Math.Sqrt(d)) / 2 / a;
        var res2 = (-b - Math.Sqrt(d)) / 2 / a;

        // Находим две точки пересечения
        var point1 = new Vector3((float) (x0 * (1 - res1) + res1 * x1), (float) (y0 * (1 - res1) + res1 * y1), (float) (z0 * (1 - res1) + res1 * z1));
        var point2 = new Vector3((float) (x0 * (1 - res2) + res2 * x1), (float) (y0 * (1 - res2) + res2 * y1), (float) (z0 * (1 - res2) + res2 * z1));

        // Берём ближайшую (видимую)
        var nearestPoint = Vector3.Subtract(firstPoint, point1).Length() < Vector3.Subtract(firstPoint, point2).Length()
            ? point1
            : point2;
        // Ищем нормаль к поверхности в этой точке
        var radiusVector = Vector3.Subtract(nearestPoint, _position);
        var radiusNormal = radiusVector / radiusVector.Length();

        return new IntersectionModel(nearestPoint, radiusNormal);
    }
}