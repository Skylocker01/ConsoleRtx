// See https://aka.ms/new-console-template for more information

using System.Numerics;
using ConsoleRtx;
using ConsoleRtx.Core;
using ConsoleRtx.Scene;
using ConsoleRtx.Scene.SceneActions;
using ConsoleRtx.Scene.SceneObjects;

var camera = Camera.GetCamera(200, 40);
camera!.Position = new Vector3(100, 50, 100);

var scene = new Scene(
    new[] {new Sphere(new Vector3(50, 0, 0), 25), new Sphere(new Vector3(-50, 0, 0), 25)},
    new Vector3(0, 60, 0), camera);

scene.SetActions(new List<Action>()
{
    () => BaseActions.RotateCamera(scene, 0.01f), 
});

var core = new RenderCore();

core.Render(scene);