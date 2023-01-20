using System.Numerics;

namespace ConsoleRtx.Camera;

public class Camera
{
    private static Camera? _camera;
    private float _pixelStep = 1f;
    private float _focalLength = 205;

    private Camera(int horResolution, int verResolution)
    {
        HorResolution = horResolution;
        VerResolution = verResolution;
    }
    
    public Vector3 Position { get; set; } = new(-100, 0, 0);
    public float FocalLength {
        get => _focalLength;
        set
        {
            if (value <= 0) throw new InvalidDataException("Focal length must be positive");
            _focalLength = value;
        }
    }
    public float PixelStep
    {
        get => _pixelStep;
        set
        {
            if (value <= 0) throw new InvalidDataException("Pixel step must be positive");
            _pixelStep = value;
        }
    }
    public int HorResolution { get; }

    public int VerResolution { get; }

    public static Camera? GetCamera(int horResolution, int verResolution)
    {
        if (_camera is null)
            _camera = new Camera(horResolution, verResolution);
        
        if (verResolution % 2 != 0 || horResolution % 2 != 0) throw new InvalidDataException("Resolution must be mean");
        
        Console.SetWindowSize(horResolution, verResolution);
        return _camera;
    }
}