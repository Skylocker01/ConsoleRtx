using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public class Scene : IScene
{
    private readonly IEnumerable<ISceneObject> _sceneObjects;
    private readonly float _symbolSizeCoefficient = (24f)/11;
    private readonly Vector3? _lightPoint;
    public Scene(IEnumerable<ISceneObject> sceneObjects, Vector3? lightPoint = null)
    {
        _sceneObjects = sceneObjects;
        _lightPoint = lightPoint;
    }

    public void Render(Camera.Camera camera)
    {
        var image = "";
        for (int i = 0 - camera.VerResolution/2; i < camera.VerResolution/2; i++)
        {
            for (int j = 0 - camera.HorResolution/2; j < camera.HorResolution/2; j++)
            {
                Vector3 firstPoint = camera.Position;
                Vector3 secondPoint = new Vector3(camera.FocalLength, i * camera.PixelStep * _symbolSizeCoefficient, j * camera.PixelStep);
                foreach (var so in _sceneObjects)
                {
                    var res = so.CalculateIntersection(firstPoint, secondPoint);
                    if (res is null || _lightPoint is null)
                    {
                        image += res is null ? " " : "@";
                        continue;
                    }
                    //Нахожу угол между нормалью и лучом к источнику света
                    var lightVector = Vector3.Subtract( _lightPoint.Value, res.IntersectionPoint);
                    var lightNormal = lightVector / lightVector.Length();
                    var angle = Vector3.Dot(lightNormal, res.NormalVector);

                    if (angle > RenderData.Angles[0])
                        image += RenderData.Symbols[0];
                    else if (angle < RenderData.Angles[0] && angle > RenderData.Angles[1])
                        image += "о";
                    else if (angle < RenderData.Angles[1] && angle > RenderData.Angles[2])
                        image += "-";
                    else
                        image += " ";
                }
            }
        }
        Console.Write(image);
    }
}