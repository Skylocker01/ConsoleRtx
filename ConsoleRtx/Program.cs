// See https://aka.ms/new-console-template for more information

using System.Numerics;
using ConsoleRtx;
using ConsoleRtx.Core;
using ConsoleRtx.Scene;
using ConsoleRtx.Scene.SceneActions;
using ConsoleRtx.Scene.SceneObjects;

var camera = Camera.GetCamera(200, 40);
camera!.Position = new Vector3(0, 30, 100);


var scene = new Scene(
    new ISceneObject[]
    {
        new Cube(new Vector3(0, 0, -20), 30),
        new Sphere(new Vector3(0, 0, 40), 20)
    },
    new Vector3(-100, 100, 10), camera);

scene.SetActions(new List<Action>()
{
    () => BaseActions.RotateCamera(scene, 0.005f), 
    //() => BaseActions.RotateLight(scene, 0.005f), 
});

var core = new RenderCore();

core.Render(scene);