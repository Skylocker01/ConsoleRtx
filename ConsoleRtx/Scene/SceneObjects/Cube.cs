using System.Numerics;
using ConsoleRtx.Models;

namespace ConsoleRtx.Scene.SceneObjects;

public class Cube : ISceneObject
{

    public Vector3 Position { get; }
    public long Side { get; }
    
    public Cube(Vector3 position, long side)
    {
        Side = side;
        Position = position;
    }
    public IntersectionModel? CalculateIntersection(Vector3 firstPoint, Vector3 secondPoint)
    {
        var potentialResults = new List<(Vector3 point, Vector3 normal)>();
        
        var xyPoint1 = CalculatePlaneIntersection(firstPoint, secondPoint, new Vector3(0, 0, -1));
        if(xyPoint1.X < Position.X + Side / 2f && xyPoint1.X > Position.X - Side / 2f && xyPoint1.Y < Position.Y + Side / 2f && xyPoint1.Y > Position.Y - Side / 2f)
            potentialResults.Add((xyPoint1, new Vector3(0, 0, -1)));
        var xyPoint2 = CalculatePlaneIntersection(firstPoint, secondPoint, new Vector3(0, 0, 1));
        if(xyPoint2.X < Position.X + Side / 2f && xyPoint2.X > Position.X - Side / 2f && xyPoint2.Y < Position.Y + Side / 2f && xyPoint2.Y > Position.Y - Side / 2f)
            potentialResults.Add((xyPoint2, new Vector3(0, 0, 1)));
        
        var yzPoint1 = CalculatePlaneIntersection(firstPoint, secondPoint, new Vector3(-1, 0, 0));
        if(yzPoint1.Z < Position.Z + Side / 2f && yzPoint1.Z > Position.Z - Side / 2f && yzPoint1.Y < Position.Y + Side / 2f && yzPoint1.Y > Position.Y - Side / 2f)
            potentialResults.Add((yzPoint1, new Vector3(-1, 0, 0)));
        var yzPoint2 = CalculatePlaneIntersection(firstPoint, secondPoint, new Vector3(1, 0, 0));
        if(yzPoint2.Z < Position.Z + Side / 2f && yzPoint2.Z > Position.Z - Side / 2f && yzPoint2.Y < Position.Y + Side / 2f && yzPoint2.Y > Position.Y - Side / 2f)
            potentialResults.Add((yzPoint2, new Vector3(1, 0, 0)));
        
        var xzPoint1 = CalculatePlaneIntersection(firstPoint, secondPoint, new Vector3(0, -1, 0));
        if(xzPoint1.Z < Position.Z + Side / 2f && xzPoint1.Z > Position.Z - Side / 2f && xzPoint1.X < Position.X + Side / 2f && xzPoint1.X > Position.X - Side / 2f)
            potentialResults.Add((xzPoint1, new Vector3(0, -1, 0)));
        var xzPoint2 = CalculatePlaneIntersection(firstPoint, secondPoint, new Vector3(0, 1, 0));
        if(xzPoint2.Z < Position.Z + Side / 2f && xzPoint2.Z > Position.Z - Side / 2f && xzPoint2.X < Position.X + Side / 2f && xzPoint2.X > Position.X - Side / 2f)
            potentialResults.Add((xzPoint2, new Vector3(0, 1, 0)));

        if (!potentialResults.Any())
            return null;
        
        var minLength = float.MaxValue;
        var indexOfMin = 0;
        for (int i = 0; i < potentialResults.Count; i++)
        {
            var length = Vector3.Subtract(potentialResults[i].point, firstPoint).Length();
            if (minLength < length)
                continue;
            indexOfMin = i;
            minLength = length;
        }

        return new IntersectionModel(potentialResults[indexOfMin].point, potentialResults[indexOfMin].normal);

    }

    private Vector3 CalculatePlaneIntersection(Vector3 firstPoint, Vector3 secondPoint, Vector3 normal)
    {
        // Использую уравнения в параметрическом виде
        // x(t) = x0*(1-t) + t*x1
        // y(t) = y0*(1-t) + t*y1
        // z(t) = z0*(1-t) + t*z1
        
        // a*x + b*y +c*z + d = 0

        // смещение центра грани относительно центра куба
        var bias = normal * Side / 2;

        var x0 = firstPoint.X;
        var x1 = secondPoint.X;
        var y0 = firstPoint.Y;
        var y1 = secondPoint.Y;
        var z0 = firstPoint.Z;
        var z1 = secondPoint.Z;

        var xc = Position.X;
        var yc = Position.Y;
        var zc = Position.Z;

        var nx = normal.X;
        var ny = normal.Y;
        var nz = normal.Z;

        var a = -nx;
        var b = -ny;
        var c = -nz;
        var d = nx * (xc + bias.X) + ny * (yc + bias.Y) + nz * (zc + bias.Z);

        var t = -(d + a * x0 + b * y0 + c * z0) / (a * (x1 - x0) + b * (y1 - y0) + c * (z1 - z0));

        var x = x0 * (1 - t) + t * x1;
        var y = y0 * (1 - t) + t * y1;
        var z = z0 * (1 - t) + t * z1;

        return new Vector3(x, y, z);
    }
}