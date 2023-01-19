using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public class Scene : IScene
{
    private readonly List<ISceneObject> _sceneObjects;
    private readonly float _symbolSizeCoefficient = (24f)/11;
    private Vector3? _lightPoint;
    public Scene(IEnumerable<ISceneObject> sceneObjects, Vector3? lightPoint = null)
    {
        _sceneObjects = sceneObjects.ToList();
        _lightPoint = lightPoint;
    }

    public void Render(Camera.Camera camera)
    {
        //Для записи в буфер консоли напрямую
        using var stdout = Console.OpenStandardOutput(camera.VerResolution * camera.VerResolution);
        var image = "";
        char[] line = new char[camera.HorResolution];
        while (true)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            
            //Прохожусь по всем объектам на сцене
            for (int k = 0; k < _sceneObjects.Count(); k++)
            {
                //Для каждого ряда пикселей
                for (int i = 0 - camera.VerResolution / 2; i < camera.VerResolution / 2; i++)
                {
                    //Параллельно вычисляю значения пиксилей
                    Parallel.For(0 - camera.HorResolution / 2, camera.HorResolution / 2, j =>
                    {
                        var horIterationNumber = j + camera.HorResolution / 2;
                        Vector3 firstPoint = camera.Position;
                        Vector3 secondPoint = new Vector3(camera.FocalLength,
                            i * camera.PixelStep * _symbolSizeCoefficient + camera.Position.Z,
                            j * camera.PixelStep + camera.Position.Y);
                        var res = _sceneObjects[k].CalculateIntersection(firstPoint, secondPoint);
                        if (res is null || _lightPoint is null)
                        {
                            line[horIterationNumber] = res is null ? ' ' : '@';
                            return;
                        }
                    
                        //Нахожу угол между нормалью и лучом к источнику света
                        var lightVector = Vector3.Subtract(_lightPoint.Value, res.IntersectionPoint);
                        var lightNormal = lightVector / lightVector.Length();
                        var angle = Vector3.Dot(lightNormal, res.NormalVector);
                    
                        if (angle > RenderData.Angles[0])
                            line[horIterationNumber] = RenderData.Symbols[0];
                        else if (angle < RenderData.Angles[0] && angle > RenderData.Angles[1])
                            line[horIterationNumber] = RenderData.Symbols[1];
                        else if (angle < RenderData.Angles[1] && angle > RenderData.Angles[2])
                            line[horIterationNumber] = RenderData.Symbols[2];
                        else
                            line[horIterationNumber] = RenderData.Symbols[3];
                    });
                    image += string.Join("", line);
                }
                
            }

            watch.Stop();
            var fps = 1000 / watch.ElapsedMilliseconds;

            byte[] buffer = image.Select(x => (byte) x).ToArray();
            stdout.Write(buffer, 0, buffer.Length);
            
            // if(_lightPoint.HasValue)
            //     RotateLight(0.1f);
        }
    }

    private void RotateLight(float radians)
    {
        if (_lightPoint is null)
            throw new InvalidOperationException("There is no light on the scene");
        
        
        var rVector = Math.Sqrt(MathF.Pow(_lightPoint.Value.X, 2) + MathF.Pow(_lightPoint.Value.Y, 2));
        var angle = Math.Acos(_lightPoint.Value.X / rVector) + radians;
        
        //x=r*cos(f)
        var x = (float)(rVector * Math.Cos(angle));
        var y = (float)(rVector * Math.Sin(angle));
        _lightPoint = new Vector3(x, y, _lightPoint.Value.Z);
    }
}