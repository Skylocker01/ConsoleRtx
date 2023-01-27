using System.Numerics;
using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public interface IScene
{
    public List<ISceneObject> SceneObjects { get; }
    public Vector3 LightPoint { get; set; }
    public Camera Camera { get; }
    public List<Action> Actions { get; }
    public void SetActions(IEnumerable<Action> actions);
}