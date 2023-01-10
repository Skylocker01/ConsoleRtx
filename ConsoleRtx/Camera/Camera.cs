using System.Numerics;

namespace ConsoleRtx.Camera;

public class Camera
{
    private static Camera? _camera;
    private int _pixelStep = 1;
    private int _horResolution;
    private int _verResolution;
    private float _focalLength = 1;
    
    private Camera()
    {}
    
    public Vector3 Position { get; set; } = new(-100, 0, 0);
    public float FocalLength {
        get => _focalLength;
        set
        {
            if (value <= 0) throw new InvalidDataException("Focal length must be positive");
            _focalLength = value;
        }
    }
    public int PixelStep
    {
        get => _pixelStep;
        set
        {
            if (value <= 0) throw new InvalidDataException("Pixel step must be positive");
            _pixelStep = value;
        }
    }
    public int HorResolution => Console.WindowWidth;

    public int VerResolution => Console.WindowHeight;

    public static Camera? GetCamera()
    {
        if (_camera is null)
            _camera = new Camera();
        return _camera;
    }
}