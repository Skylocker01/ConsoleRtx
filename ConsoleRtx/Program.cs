// See https://aka.ms/new-console-template for more information

using System.Numerics;
using ConsoleRtx.Camera;
using ConsoleRtx.Scene;
using ConsoleRtx.Scene.SceneObjects;

var camera = Camera.GetCamera(200, 40);
camera!.Position = new Vector3(200, 0, 0);


var scene = new Scene(new[] {new Sphere(new Vector3(0, 0, 0), 25)}, new Vector3(40, 40, 0), camera);


scene.Render();
Console.ReadKey();