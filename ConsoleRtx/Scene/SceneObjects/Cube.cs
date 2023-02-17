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

        var normal = Vector3.Zero;
        //Проходимся по трём осям
        for (int i = 0; i < 3; i++)
        {
            // Мне нужно задать условия на плоскости, перпендикулярной к текущей нормали, для этого нахожу индексы соответствующих осей
            // (Для оси Х мне нужны оси Y и Z, т.е. для индекса 0 - индексы 1 и 2, для 1 - индексы 0 и 2, для 2 - индексы 0 и 1)
            // Мамкин математик придумал соответствующие функции
            var firstPerpendicularIndex = (int) Math.Pow(0, i);
            var secondPerpendicularIndex =
                (int) (firstPerpendicularIndex + i + Math.Pow(-1, firstPerpendicularIndex + i + 1));
            // Проходимся по двум возможным направлениям нормалей в точках пересечения
            for (int j = 0; j < 2; j++)
            {
                // Задаю нормаль для данной плоскости
                normal[i] = MathF.Pow(-1, j);
                var point = CalculatePlaneIntersection(firstPoint, secondPoint, normal);
                // Если точка лежит в пределах стороны куба - добавляю её
                if (point[firstPerpendicularIndex] < Position[firstPerpendicularIndex] + Side / 2f &&
                    point[firstPerpendicularIndex] > Position[firstPerpendicularIndex] - Side / 2f &&
                    point[secondPerpendicularIndex] < Position[secondPerpendicularIndex] + Side / 2f &&
                    point[secondPerpendicularIndex] > Position[secondPerpendicularIndex] - Side / 2f)
                    potentialResults.Add((point, normal));
            }

            // У прямой может быть не более двух точек пересечения с трёхмерным телом
            if (potentialResults.Count() == 2)
                break;
            normal = Vector3.Zero;
        }

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