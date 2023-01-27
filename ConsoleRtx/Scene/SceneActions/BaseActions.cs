using System.Numerics;

namespace ConsoleRtx.Scene.SceneActions;

public static class BaseActions
{
    
    public static void RotateLight(IScene scene, float radians)
    {
        var rVector =
            (float) Math.Sqrt(MathF.Pow(scene.LightPoint.X, 2) + MathF.Pow(scene.LightPoint.Z, 2));
        var angle = Math.Atan2(scene.LightPoint.Z, scene.LightPoint.X) + radians;

        //x=r*cos(f)
        var x = (float) (rVector * Math.Cos(angle));
        var y = (float) (rVector * Math.Sin(angle));
        scene.LightPoint = new Vector3(x, scene.LightPoint.Y, y);
    }
    
    public static void RotateCamera(IScene scene, float radians)
    {
        var rVector =
            (float) Math.Sqrt(MathF.Pow(scene.Camera.Position.X, 2) + MathF.Pow(scene.Camera.Position.Z, 2));
        var angle = Math.Atan2(scene.Camera.Position.Z, scene.Camera.Position.X) + radians;

        //x=r*cos(f)
        var x = (float) (rVector * Math.Cos(angle));
        var y = (float) (rVector * Math.Sin(angle));
        scene.Camera.Position = new Vector3(x, scene.Camera.Position.Y, y);
    }
}