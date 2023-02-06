using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene;

namespace ConsoleRtx.Core;

public class RenderCore
{
    public virtual void Render(IScene scene)
    {
        char[][] imageArr = new char[scene.Camera.VerResolution][];
        while (true)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            //Для каждого ряда пикселей
            Parallel.For(0 - scene.Camera.VerResolution / 2, scene.Camera.VerResolution / 2,
                //new ParallelOptions() {MaxDegreeOfParallelism = 1},
                i =>
                {
                    char[] line = new char[scene.Camera.HorResolution];
                    //Создаю отдельный индексатор с 0, чтобы удобнее было работать с массивом
                    var verIterationNumber = i + scene.Camera.VerResolution / 2;
                    //Параллельно вычисляю значения пиксилей
                    Parallel.For(0 - scene.Camera.HorResolution / 2, scene.Camera.HorResolution / 2,
                        //new ParallelOptions() {MaxDegreeOfParallelism = 1},
                        j =>
                        {
                            var horIterationNumber = j + scene.Camera.HorResolution / 2;
                            Vector3 viewPoint = Vector3.Zero;
                            Vector3 pixelPoint =
                                scene.Camera.GetCameraPixelCoord(i, j, viewPoint);

                            List<IntersectionModel> allIntersections = new();

                            //Прохожусь по всем объектам на сцене
                            for (int k = 0; k < scene.SceneObjects.Count(); k++)
                            {
                                var res = scene.SceneObjects[k]
                                    .CalculateIntersection(scene.Camera.Position, pixelPoint);
                                if (res != null)
                                    allIntersections.Add(res);
                            }

                            //Нахожу ближайшую к камере точку пересечения
                            IntersectionModel closestPoint = null;
                            float currMin = float.MaxValue;
                            for (int k = 0; k < allIntersections.Count; k++)
                            {
                                var range = Vector3.Distance(allIntersections[k].IntersectionPoint,
                                    scene.Camera.Position);
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
                            var lightVector = Vector3.Subtract(scene.LightPoint, closestPoint.IntersectionPoint);
                            var lightNormal = lightVector / lightVector.Length();
                            var cos = Vector3.Dot(lightNormal, closestPoint.NormalVector);

                            //В зависимости от угла рассчитываю яркость пикселя
                            for (int p = 0; p < RenderData.Angles.Length - 1; p++)
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
            
            // Выполняю все запланированные действия над сценой
            for (int i = 0; i < scene.Actions?.Count; i++)
            {
                scene.Actions[i].Invoke();
            }
            
            watch.Stop();
            PrintDebugInfo(scene, watch.ElapsedMilliseconds, imageArr);

            //Для записи в буфер консоли напрямую
            using var stdout = Console.OpenStandardOutput(scene.Camera.VerResolution * scene.Camera.VerResolution);
            byte[] buffer = imageArr.SelectMany(x => x.Select(y => (byte) y)).ToArray();
            stdout.Write(buffer, 0, buffer.Length);
            //Console.ReadKey();
        }
    }

    protected void PrintDebugInfo(IScene scene, long millisecondsForFrame, char[][] imageArr)
    {
        var fps = 1000.0 / (millisecondsForFrame + 1);
        var fpsString = $"fps: {fps}";
        WriteToOutputLine(fpsString, imageArr[2]);
        WriteToOutputLine($"scene.LightPoint X: {scene.LightPoint.X}, Y: {scene.LightPoint.Y}, Z: {scene.LightPoint.Z}", imageArr[3]);
        WriteToOutputLine($"Camera X: {scene.Camera.Position.X}, Y: {scene.Camera.Position.Y}, Z: {scene.Camera.Position.Z}",
            imageArr[4]);
        WriteToOutputLine(
            $"Object X: {scene.SceneObjects.First().Position.X}, Y: {scene.SceneObjects.First().Position.Y}, Z: {scene.SceneObjects.First().Position.Z}",
            imageArr[5]);
    }
    
    
    protected void WriteToOutputLine(string str, in char[] arr)
    {
        for (int i = 0; i < str.Length; i++)
        {
            arr[i] = str[i];
        }
    }

}