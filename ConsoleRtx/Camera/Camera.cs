using System.Numerics;

namespace ConsoleRtx.Camera;

public class Camera
{
    private static Camera? _camera;
    private int _pixelStep = 1;
    private int _horResolution;
    private int _verResolution;
    
    private Camera()
    {}
    
    public Vector3 Position { get; set; } = new(-100, 0, 0);
    public int PixelStep
    {
        get => _pixelStep;
        set
        {
            if (value <= 0) throw new InvalidDataException("Pixel step must be positive");
            _pixelStep = value;
        }
    }
    public int HorResolution
    {
        get => Console.WindowWidth;
        private set
        {
            if (value % 2 != 0)
                throw new InvalidDataException("Resolution must be mean");
            _horResolution = value;
        }
    }
    public int VerResolution
    {
        get => Console.WindowHeight;
        private set
        {
            if (value % 2 != 0)
                throw new InvalidDataException("Resolution must be mean");
            _verResolution = value;
        }
    }

    public static Camera? GetCamera()//int horizontalResolution, int verticalResolution)
    {
        if (_camera is null)
            _camera = new Camera();
        // _camera.HorResolution = horizontalResolution;
        // _camera.VerResolution = verticalResolution;
        // Console.SetWindowSize(horizontalResolution, verticalResolution);
        return _camera;
    }
}