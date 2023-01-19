// See https://aka.ms/new-console-template for more information

using System.Numerics;
using ConsoleRtx.Camera;
using ConsoleRtx.Scene;
using ConsoleRtx.Scene.SceneObjects;

var scene = new Scene(new[] {new Sphere(new Vector3(0, 0, 0), 20)}, new Vector3(-50, 30, -50));

var camera = Camera.GetCamera();
camera!.Position = new Vector3(-200, 0, 0);

scene.Render(camera);
Console.ReadKey();