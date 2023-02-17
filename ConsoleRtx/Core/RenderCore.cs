using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene;

namespace ConsoleRtx.Core;

public class RenderCore
{
    //Количество переотражений луча
    public int ReflectionsCount = 2;
    //Коэффициент ослабления луча при переотражении
    public float ReflectionCoefficient = 0.5f;

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
                            //Создаю отдельный индексатор с 0, чтобы удобнее было работать с массивом
                            var horIterationNumber = j + scene.Camera.HorResolution / 2;

                            // Точка, в сторону который смотрит камера
                            Vector3 viewPoint = Vector3.Zero;
                            // Координаты пикселя на виртуальном холсте
                            Vector3 pixelPoint =
                                scene.Camera.GetCameraPixelCoord(i, j, viewPoint);

                            // Единичнный вектор, отвечающий испущенному лучу
                            Vector3 viewVector = pixelPoint - scene.Camera.Position;
                            viewVector /= viewVector.Length();
                            // Координаты точки, из которой испускается луч
                            Vector3 sourcePoint = scene.Camera.Position;
                            //Здесь храним сумму косинусов для переотражённого луча, т.е. яркость пикселя
                            float resultCosSum = 0;
                            // Все найденные точки пересечения
                            List<IntersectionModel> allIntersections = new();

                            for (int r = 0; r <= ReflectionsCount; r++)
                            {
                                //Прохожусь по всем объектам на сцене
                                for (int k = 0; k < scene.SceneObjects.Count(); k++)
                                {
                                    var res = scene.SceneObjects[k]
                                        .CalculateIntersection(sourcePoint, sourcePoint + viewVector);
                                    if (res != null)
                                        allIntersections.Add(res);
                                }

                                //Нахожу ближайшую точку пересечения с внешней стороной объектов в направлении взгляда
                                IntersectionModel closestPoint = null;
                                float currMin = float.MaxValue;
                                for (int k = 0; k < allIntersections.Count; k++)
                                {
                                    // Если направление нормали совпадает с направлением луча - мы не должны видеть эту точку
                                    if (Vector3.Dot(allIntersections[k].NormalVector, viewVector) > 0)
                                        continue;

                                    var range = Vector3.Distance(allIntersections[k].IntersectionPoint,
                                        sourcePoint);
                                    if (range < currMin)
                                    {
                                        currMin = range;
                                        closestPoint = allIntersections[k];
                                    }
                                }

                                if (closestPoint is null && r == 0)
                                {
                                    // Если начальный луч не попадает ни на один объект - точка красится в самый тёмный цвет
                                    line[horIterationNumber] = RenderData.Symbols.Last();
                                    return;
                                }
                                // Если переотражённый луч не попал на объекты - прекращаем цикл переотражений
                                if (closestPoint is null && r != 0)
                                {
                                    break;
                                }

                                //Нахожу угол между нормалью и лучом к источнику света
                                var lightVector = Vector3.Subtract(scene.LightPoint, closestPoint.IntersectionPoint);
                                var lightNormalizedVector = lightVector / lightVector.Length();
                                var lightCos = Vector3.Dot(lightNormalizedVector, closestPoint.NormalVector);
                                // Мы не можем ослабевать луч при переотражениях - прибаляем только положительные значения
                                resultCosSum += lightCos < 0 && r > 0 ? 
                                    0 : 
                                    // После каждого переотражения луч слегка ослабевает
                                    lightCos * MathF.Pow(ReflectionCoefficient, r);

                                allIntersections.Clear();
                                // Теперь выпускаю новый луч из точки пересечения с поверхностью
                                sourcePoint = closestPoint.IntersectionPoint;
                                var cos = Vector3.Dot(closestPoint.NormalVector, -viewVector);
                                // Тут именно математически нельзя, чтобы cos == 1
                                if (cos == 1)
                                    viewVector = -viewVector;
                                else
                                {
                                    // Новый вектор, отвечающий отражённому лучу
                                    viewVector += closestPoint.NormalVector * 2 * cos;
                                }
                            }


                            //В зависимости от суммы косинусов переотражений рассчитываю яркость пикселя
                            for (int p = 0; p < RenderData.Angles.Length; p++)
                            {
                                if (resultCosSum > RenderData.Angles[p])
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
        WriteToOutputLine($"scene.LightPoint X: {scene.LightPoint.X}, Y: {scene.LightPoint.Y}, Z: {scene.LightPoint.Z}",
            imageArr[3]);
        WriteToOutputLine(
            $"Camera X: {scene.Camera.Position.X}, Y: {scene.Camera.Position.Y}, Z: {scene.Camera.Position.Z}",
            imageArr[4]);
        // for (int i = 0; i < scene.SceneObjects.Count; i++)
        // {
        //     WriteToOutputLine(
        //         $"Object {i} Position X: {scene.SceneObjects[i].Position.X}, Y: {scene.SceneObjects[i].Position.Y}, Z: {scene.SceneObjects[i].Position.Z}",
        //         imageArr[5 + i]);
        // }
    }


    protected void WriteToOutputLine(string str, in char[] arr)
    {
        for (int i = 0; i < str.Length; i++)
        {
            arr[i] = str[i];
        }
    }
}