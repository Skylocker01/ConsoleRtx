using System.Numerics;

namespace ConsoleRtx;

public class Camera
{
    private static Camera? _camera;
    private float _pixelStep = 1f;
    private readonly float _symbolSizeCoefficient = (24f) / 11;
    private float _focalLength = 110;

    private Camera(int horResolution, int verResolution)
    {
        HorResolution = horResolution;
        VerResolution = verResolution;
    }
    
    public Vector3 Position { get; set; } = new(100, 0, 0);
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

    public Vector3 GetCameraPixelCoord(int row, int column, Vector3 viewPoint)
    {
        Vector3 pixelPoint = new Vector3(column * PixelStep,
            -row * PixelStep * _symbolSizeCoefficient,
            _focalLength);
        return TransformToSceneCoordinateSystem(viewPoint, pixelPoint);
    }
    /// <summary>
    /// Возвращает координаты точки в СК сцены по направлению взгляда камеры и координат в СК камеры
    /// </summary>
    /// <param name="viewPoint">Направление взгляда камеры</param>
    /// <param name="pointInCameraSystem">Координата точки в СК камеры</param>
    /// <returns>Координата точки в СК сцены</returns>
    /// <exception cref="InvalidDataException"></exception>
    public Vector3 TransformToSceneCoordinateSystem(Vector3 viewPoint, Vector3 pointInCameraSystem)
    {
        // Мне пока впадлу решать такое уравнение, так что:
        if (viewPoint.X == _camera.Position.X && viewPoint.Z == _camera.Position.Z)
            throw new InvalidDataException("View point can not be right above or under the camera");
        
        var xc = _camera.Position.X;
        var yc = _camera.Position.Y;
        var zc = _camera.Position.Z;
        // Направление взгляда камеры в СК сцены - ось Z СК камеры
        var kc = viewPoint - Position;
        Vector3 ic;
        // Ищем единичный вектор оси X СК камеры в СК сцены
        if (kc.Z != 0 && kc.X != 0)
        {
            // kcx * x + kcy * y + kcz * z - kcx * xc - kcy * yc - kcz * zc= 0
            // y = yc, потому что прямая всегда перпендикулярна оси Y
            // z = zc + 1
            // kcz + kcx * xk - kcx * xc = 0
            var xk = (kc.X * xc - kc.Z) / kc.X;
            var icx = (xk - xc);
            // Найденной прямой соответствуют 2 направления векторов, выбираю согласно ориентации оси Zc
            icx *=  Math.Sign(kc.Z) * Math.Sign(icx);
            var icz = -Math.Sign(kc.X);
            
            ic = new Vector3(icx, 0, icz);
        }
        else if (kc.X == 0)
        {
            ic = kc.Z > 0 ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
        }
        else
        {
            ic = kc.X > 0 ? new Vector3(0, 0, -1) : new Vector3(0, 0, 1);
        }
        // Ищем единичный вектор оси Y СК камеры в СК сцены
        Vector3 jc;
        // Геометрически выводится
        jc.Y = Single.Sqrt(MathF.Pow(kc.X, 2) + MathF.Pow(kc.Z, 2));
        if (kc.Z != 0)
        {
            jc.Z = Single.Sqrt(MathF.Pow(kc.Y, 2) / (1 + MathF.Pow(kc.X / kc.Z, 2))) * Math.Sign(kc.Z);
            jc.X = jc.Z * kc.X / kc.Z;
        }
        else
        {
            jc.Z = 0;
            jc.X = kc.Y;
        }

        ic /= ic.Length();
        jc /= jc.Length();
        kc /= kc.Length();

        // Шарп не умеет в матрицы 3х3, не хочу левых фреймворков ради одного выражения, так что выкручиваюсь так
        var transitionMatrix = new Matrix4x4(ic.X, jc.X, kc.X, 0, ic.Y, jc.Y, kc.Y, 0, ic.Z, jc.Z, kc.Z, 0, 0, 0, 0, 1);
        var result = new Vector3();
        
        //Переходим от СК камеры к СК сцены
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                //Сразу выполняю сдвиг согласно положению СК камеры в СК сцены
                result[i] += transitionMatrix[i, j] * pointInCameraSystem[j];
            }
        }

        result += Position;

        return result;
    }
    
}