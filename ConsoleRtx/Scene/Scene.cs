using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public class Scene : IScene
{
    private readonly List<ISceneObject> _sceneObjects;
    private Vector3 _lightPoint;
    private readonly Camera.Camera _camera;

    public Scene(IEnumerable<ISceneObject> sceneObjects, Vector3 lightPoint, Camera.Camera camera)
    {
        _sceneObjects = sceneObjects.ToList();
        _lightPoint = lightPoint;
        _camera = camera;
    }

    public void Render()
    {
        char[][] imageArr = new char[_camera.VerResolution][];
        while (true)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            //Для каждого ряда пикселей
            Parallel.For(0 - _camera.VerResolution / 2, _camera.VerResolution / 2, i =>
            {
                char[] line = new char[_camera.HorResolution];
                //Создаю отдельный индексатор с 0, чтобы удобнее было работать с массивом
                var verIterationNumber = i + _camera.VerResolution / 2;
                //Параллельно вычисляю значения пиксилей
                Parallel.For(0 - _camera.HorResolution / 2, _camera.HorResolution / 2, j =>
                {
                    var horIterationNumber = j + _camera.HorResolution / 2;
                    Vector3 viewPoint = Vector3.Zero;
                    Vector3 firstPoint = _camera.Position;
                    Vector3 secondPoint =
                        _camera.GetCameraPixelCoord(i, j, viewPoint);

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
                        var range = Vector3.Distance(allIntersections[k].IntersectionPoint, _camera.Position);
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

                    //В зависимости от угла рассчитываю яркость пикселя
                    for (int p = 0; p < RenderData.Angles.Length - 2; p++)
                    {
                        if (cos < RenderData.Angles[p] && cos > RenderData.Angles[p + 1])
                        {
                            line[horIterationNumber] = RenderData.Symbols[p];
                            break;
                        }
                    }
                });

                imageArr[verIterationNumber] = line;
            });

            RotateLight(0.01f);
            //((Sphere) _sceneObjects.First()).MoveX(100);
            //((Sphere)_sceneObjects.First()).MoveZ(50);
            watch.Stop();
            
            
            PrintDebugInfo(watch.ElapsedMilliseconds, imageArr);

            //Для записи в буфер консоли напрямую
            using var stdout = Console.OpenStandardOutput(_camera.VerResolution * _camera.VerResolution);
            byte[] buffer = imageArr.SelectMany(x => x.Select(y => (byte) y)).ToArray();
            stdout.Write(buffer, 0, buffer.Length);
            //Console.ReadKey();
        }
    }

    private void PrintDebugInfo(long millisecondsForFrame, char[][] imageArr)
    {
        var fps = 1000.0 / (millisecondsForFrame + 1);
        var fpsString = $"fps: {fps}";
        WriteToOutputLine(fpsString, imageArr[2]);
        WriteToOutputLine($"LightPoint X: {_lightPoint.X}, Y: {_lightPoint.Y}, Z: {_lightPoint.Z}", imageArr[3]);
        WriteToOutputLine($"_camera X: {_camera.Position.X}, Y: {_camera.Position.Y}, Z: {_camera.Position.Z}",
            imageArr[4]);
        WriteToOutputLine(
            $"Object X: {_sceneObjects.First().Position.X}, Y: {_sceneObjects.First().Position.Y}, Z: {_sceneObjects.First().Position.Z}",
            imageArr[5]);
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