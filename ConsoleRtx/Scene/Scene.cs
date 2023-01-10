using System.Numerics;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public class Scene : IScene
{
    private readonly IEnumerable<ISceneObject> _sceneObjects;
    private const float _symbolSizeCoefficient = 2;
    public Scene(IEnumerable<ISceneObject> sceneObjects)
    {
        _sceneObjects = sceneObjects;
    }

    public void Render(Camera.Camera camera)
    {
        var image = "";
        for (int i = 0 - camera.VerResolution/2; i < camera.VerResolution/2; i++)
        {
            for (int j = 0 - camera.HorResolution/2; j < camera.HorResolution/2; j++)
            {
                Vector3 firstPoint = new Vector3(0, i * camera.PixelStep * _symbolSizeCoefficient, j * camera.PixelStep);
                Vector3 secondPoint = new Vector3(1, i * camera.PixelStep * _symbolSizeCoefficient, j * camera.PixelStep);
                foreach (var so in _sceneObjects)
                {
                    var res = so.CalculateIntersection(firstPoint, secondPoint);
                    image += res ? "@" : " ";
                }
            }
        }
        Console.Write(image);
    }
}