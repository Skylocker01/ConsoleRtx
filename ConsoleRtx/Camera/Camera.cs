using System.Numerics;

namespace ConsoleRtx.Camera;

public class Camera
{
    private static Camera? _camera;
    private float _pixelStep = 1f;
    private readonly float _symbolSizeCoefficient = (24f) / 11;

    private Camera(int horResolution, int verResolution)
    {
        HorResolution = horResolution;
        VerResolution = verResolution;
    }
    
    public Vector3 Position { get; set; } = new(-100, 0, 0);

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
        Vector3 pixelPoint = new Vector3(_camera.Position.X,
            row * _camera.PixelStep * _symbolSizeCoefficient,
            column * _camera.PixelStep);
        return TransformToSceneCoordinateSystem(viewPoint, pixelPoint);
    }
    /// <summary>
    /// Возвращает координаты точки в СК сцены по направлению взгляда камеры и координат в СК камеры
    /// </summary>
    /// <param name="viewPoint">Направление взгляда камеры</param>
    /// <param name="pointInCameraSystem">Координата точки в СК камеры</param>
    /// <returns>Координата точки в СК сцены</returns>
    /// <exception cref="InvalidDataException"></exception>
    private Vector3 TransformToSceneCoordinateSystem(Vector3 viewPoint, Vector3 pointInCameraSystem)
    {
        // Мне пока впадлу решать такое уравнение, так что:
        if (viewPoint.X == _camera.Position.X && viewPoint.Y == _camera.Position.Y)
            throw new InvalidDataException("View point can not be right above or under the camera");
        
        var xc = _camera.Position.X;
        var yc = _camera.Position.Y;
        var zc = _camera.Position.Z;
        // Направление взгляда камеры в СО сцены
        var ic = viewPoint - Position;
        Vector3 jc;
        // Ищем единичный вектор камеры jc в СО сцены
        if (ic.Y != 0)
        {
            // icx * x + icy * y + icz * z - icx * xc - icy * yc - icz * zc= 0
            // z = zc; x = xc + 1
            // icx + icy * y - icy * yc= 0
            // yk = (- icx + icy * yc) / icy
            var yk = (- ic.X + ic.Y * yc) / ic.Y;
            jc = new Vector3(xc + 1, yk, zc) - _camera.Position;
        }
        else
        {
            // // icx * x + icz * z - icx * xc - icy * yc - icz * zc= 0
            // // z = 0
            // // xk = (icx * xc + icy * yc + icz * zc) / icx
            // var xk = (ic.X * xc + ic.Y * yc + ic.Z * zc) / ic.X;
            // jc = new Vector3(xk, 0, 0) - _camera.Position;
            jc = ic.X > 0 ? new Vector3(0, 1, 0) : new Vector3(0, -1, 0);
        }
        Vector3 kc;
        if (ic.X != 0)
        {
            // icx * x + icy * y + izy * z - icx * xc - icy * yc - icz * zc= 0
            // jcx * x + jcy * y + jcz * z - jcx * xc - jcy * yc - jcz * zc= 0
            // zk = zc + 1;
            // icx * x + icy * y - icx * xc - icy * yc + icz= 0 => x = - icy / icx * y - (- icx * xc - icy * yc + icz) / icx
            // - jcx * icy / icx * y + jcy * y - (- icx * xc - icy * yc + icz) * jcx / icx + jcz - jcx * xc - jcy * yc= 0
            // yk = ((- icx * xc - icy * yc + icz) * jcx / icx - (jcz - jcx * xc - jcy * yc)) / (- jcx * icy / icx + jcy)
            // xk = - icy / icx * yk - (- icx * xc - icy * yc + icz) / icx

            var zk = zc + 1;
            var yk = ((- ic.X * xc - ic.Y * yc + ic.Z) * jc.X / ic.X - (jc.Z - jc.X * xc - jc.Y * yc)) / (- jc.X * ic.Y / ic.X + jc.Y);
            var xk = -ic.Y / ic.X * yk - (-ic.X * xc - ic.Y * yc + ic.Z) / ic.X;
            kc = new Vector3(xk, yk, zk) - _camera.Position;
        }
        else
        {
            // icy * y + izy * z - icx * xc - icy * yc - icz * zc= 0
            // jcx * x + jcy * y + jcz * z - jcx * xc - jcy * yc - jcz * zc= 0
            // zk = zc + 1;
            // icy * y - icy * yc + icz= 0 => yk = (icy * yc - icz) / icy
            // jcx * x + jcy * yk + jcz - jcx * xc - jcy * yc= 0 =>  xk = -(jcy * yk + jcz - jcx * xc - jcy * yc) / jcx
            var zk = zc + 1;
            var yk = (ic.Y * yc - ic.Z) / ic.Y;
            var xk = -(jc.Y * yk + jc.Z - jc.X * xc - jc.Y * yc) / jc.X;
            kc = new Vector3(xk, yk, zk) - _camera.Position;
        }

        ic /= ic.Length();
        jc /= jc.Length();
        kc /= kc.Length();

        // Шарп не умеет в матрицы 3х3, не хочу левых фреймворков ради одного выражения, так что выкручиваюсь так
        var matrixToInvert = new Matrix4x4(ic.X, jc.X, kc.X, 0, ic.Y, jc.Y, kc.Y, 0, ic.Z, jc.Z, kc.Z, 0, 0, 0, 0, 1);
        Matrix4x4.Invert(matrixToInvert, out var transitionMatrix);
        var result = new Vector3();
        
        //Переходим от базиса камеры к базису сцены
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i] += transitionMatrix[i, j] * pointInCameraSystem[i];
            }
        }
        
        //Выполняем сдвиг СО
        result += _camera.Position;


        return result;
    }
    
}