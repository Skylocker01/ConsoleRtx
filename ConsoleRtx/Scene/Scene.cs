using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public class Scene : IScene
{
    private readonly List<ISceneObject> _sceneObjects;
    private readonly float _symbolSizeCoefficient = (24f) / 11;
    private Vector3 _lightPoint;

    public Scene(IEnumerable<ISceneObject> sceneObjects, Vector3 lightPoint)
    {
        _sceneObjects = sceneObjects.ToList();
        _lightPoint = lightPoint;
    }

    public void Render(Camera.Camera camera)
    {
        char[][] imageArr = new char[camera.VerResolution][];
        while (true)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            //Для каждого ряда пикселей
            Parallel.For(0 - camera.VerResolution / 2, camera.VerResolution / 2, i =>
            {
                char[] line = new char[camera.HorResolution];
                //Создаю одильный индексатор с 0, чтобы удобнее было работать с массивом
                var verIterationNumber = i + camera.VerResolution / 2;
                //Параллельно вычисляю значения пиксилей
                Parallel.For(0 - camera.HorResolution / 2, camera.HorResolution / 2, j =>
                {
                    var horIterationNumber = j + camera.HorResolution / 2;
                    Vector3 firstPoint = camera.Position;
                    Vector3 secondPoint = new Vector3(camera.FocalLength + camera.Position.X,
                        i * camera.PixelStep * _symbolSizeCoefficient + camera.Position.Y,
                        j * camera.PixelStep + camera.Position.Z);

                    List<IntersectionModel> allIntersections = new();

                    //Прохожусь по всем объектам на сцене
                    for (int k = 0; k < _sceneObjects.Count(); k++)
                    {
                        var res = _sceneObjects[k].CalculateIntersection(firstPoint, secondPoint);
                        if (res != null)
                            allIntersections.Add(res);
                    }

                    //Нахожу ближайшую к камере точку пересечения
                    IntersectionModel closestPoint = null;
                    float currMin = float.MaxValue;
                    for (int k = 0; k < allIntersections.Count; k++)
                    {
                        var range = Vector3.Distance(allIntersections[k].IntersectionPoint, camera.Position);
                        if (range < currMin)
                        {
                            currMin = range;
                            closestPoint = allIntersections[k];
                        }
                    }

                    if (closestPoint is null)
                    {
                        line[horIterationNumber] = ' ';
                        return;
                    }

                    //Нахожу угол между нормалью и лучом к источнику света
                    var lightVector = Vector3.Subtract(_lightPoint, closestPoint.IntersectionPoint);
                    var lightNormal = lightVector / lightVector.Length();
                    var cos = Vector3.Dot(lightNormal, closestPoint.NormalVector);

                    if (cos > RenderData.Angles[0])
                        line[horIterationNumber] = RenderData.Symbols[0];
                    else if (cos < RenderData.Angles[0] && cos > RenderData.Angles[1])
                        line[horIterationNumber] = RenderData.Symbols[1];
                    else if (cos < RenderData.Angles[1] && cos > RenderData.Angles[2])
                        line[horIterationNumber] = RenderData.Symbols[2];
                    else if (cos < RenderData.Angles[2] && cos > RenderData.Angles[3])
                        line[horIterationNumber] = RenderData.Symbols[3];
                    else
                        line[horIterationNumber] = RenderData.Symbols[4];
                });

                imageArr[verIterationNumber] = line;
            });

            RotateLight(0.01f);
            ((Sphere) _sceneObjects.First()).MoveX(100);
            ((Sphere)_sceneObjects.First()).MoveZ(100);


            watch.Stop();
            var fps = 1000.0 / (watch.ElapsedMilliseconds + 1);
            var fpsString = $"fps: {fps}";
            WriteToOutputLine(fpsString, imageArr[2]);
            WriteToOutputLine($"LightPoint X: {_lightPoint.X}, Y: {_lightPoint.Y}, Z: {_lightPoint.Z}", imageArr[3]);
            WriteToOutputLine($"Camera X: {camera.Position.X}, Y: {camera.Position.Y}, Z: {camera.Position.Z}",
                imageArr[4]);
            WriteToOutputLine(
                $"Object X: {_sceneObjects.First().Position.X}, Y: {_sceneObjects.First().Position.Y}, Z: {_sceneObjects.First().Position.Z}",
                imageArr[5]);

            //Для записи в буфер консоли напрямую
            using var stdout = Console.OpenStandardOutput(camera.VerResolution * camera.VerResolution);
            //byte[] buffer = image.Select(x => (byte) x).ToArray();
            byte[] buffer = imageArr.SelectMany(x => x.Select(y => (byte) y)).ToArray();
            stdout.Write(buffer, 0, buffer.Length);
            //Console.ReadKey();
        }
    }

    private void WriteToOutputLine(string str, in char[] arr)
    {
        for (int i = 0; i < str.Length; i++)
        {
            arr[i] = str[i];
        }
    }

    private void RotateLight(float radians)
    {
        var rVector =
            (float) Math.Sqrt(MathF.Pow(_lightPoint.X, 2) + MathF.Pow(_lightPoint.Z, 2));
        var angle = Math.Atan2(_lightPoint.Z, _lightPoint.X) + radians;

        //x=r*cos(f)
        var x = (float) (rVector * Math.Cos(angle));
        var y = (float) (rVector * Math.Sin(angle));
        _lightPoint = new Vector3(x, _lightPoint.Y, y);
    }
}