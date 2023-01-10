using ConsoleRtx.Scene.SceneObjects;

namespace ConsoleRtx.Scene;

public interface IScene
{
    void Render(Camera.Camera camera);
}