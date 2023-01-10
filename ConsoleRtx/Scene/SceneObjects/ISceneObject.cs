using System.Numerics;

namespace ConsoleRtx.Scene.SceneObjects;

public interface ISceneObject
{
    bool CalculateIntersection(Vector3 firstPoint, Vector3 secondPoint);
}