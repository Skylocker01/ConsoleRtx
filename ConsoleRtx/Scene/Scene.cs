using System.Numerics;
using ConsoleRtx.Models;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public class Scene : IScene
{
    public List<ISceneObject> SceneObjects { get;  private set; }
    public Vector3 LightPoint { get;  set; }
    public Camera Camera { get;  private set; }
    public List<Action>? Actions { get; private set; }

    public Scene(IEnumerable<ISceneObject> sceneObjects, Vector3 lightPoint, Camera camera)
    {
        SceneObjects = sceneObjects.ToList();
        LightPoint = lightPoint;
        Camera = camera;
    }

    public void SetActions(IEnumerable<Action> actions)
    {
        Actions = actions?.ToList();
    }
}