using System.Numerics;
using ConsoleRtx.Models;

namespace ConsoleRtx.Scene.SceneObjects;

public interface ISceneObject
{
    Vector3 Position { get; }
    IntersectionModel? CalculateIntersection(Vector3 firstPoint, Vector3 secondPoint);
}