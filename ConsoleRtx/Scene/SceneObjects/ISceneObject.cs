using System.Numerics;
using ConsoleRtx.Models;

namespace ConsoleRtx.Scene.SceneObjects;

public interface ISceneObject
{
    IntersectionModel? CalculateIntersection(Vector3 firstPoint, Vector3 secondPoint);
}