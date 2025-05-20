using System.Numerics;

namespace SearchitLibrary.Graphics;

public class Camera
{
    private Vector3 _direction;
    private float _pitch;
    private Vector3 _right;
    private float _yaw;

    public Camera(Vector3 position, Vector3 target, Vector3 up, float fieldOfView, float aspectRatio, float nearPlane,
        float farPlane)
    {
        Position = position;
        Target = target;
        Up = up;
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
        NearPlane = nearPlane;
        FarPlane = farPlane;

        // Initialize yaw and pitch from initial target position
        var initialDirection = Vector3.Normalize(target - position);
        _pitch = (float)Math.Asin(initialDirection.Y);
        _yaw = (float)Math.Atan2(initialDirection.X, initialDirection.Z);
        _direction = initialDirection;

        // Initialize right vector
        _right = Vector3.Normalize(Vector3.Cross(_direction, Up));
    }

    public Vector3 Position { get; set; }

    public Vector3 Target { get; set; }

    public Vector3 Up { get; set; }

    public Vector3 Direction => _direction;

    public Vector3 Right => _right;

    public float MoveSpeed { get; set; } = 5.0f;

    public float FieldOfView { get; set; }

    public float AspectRatio { get; set; }

    public float NearPlane { get; set; }

    public float FarPlane { get; set; }

    public float Yaw
    {
        get => _yaw;
        set
        {
            _yaw = value;
            UpdateDirectionAndTarget();
        }
    }

    public float Pitch
    {
        get => _pitch;
        set
        {
            // Clamp pitch to avoid gimbal lock
            _pitch = Math.Clamp(value, -MathF.PI / 2.0f + 0.1f, MathF.PI / 2.0f - 0.1f);
            UpdateDirectionAndTarget();
        }
    }

    private void UpdateDirectionAndTarget()
    {
        // Calculate new direction vector from yaw and pitch
        _direction = new Vector3(
            (float)(Math.Sin(_yaw) * Math.Cos(_pitch)),
            (float)Math.Sin(_pitch),
            (float)(Math.Cos(_yaw) * Math.Cos(_pitch))
        );

        // Normalize the direction vector
        _direction = Vector3.Normalize(_direction);

        // Update right vector
        _right = Vector3.Normalize(Vector3.Cross(_direction, Up));

        // Update target position based on position and direction
        Target = Position + _direction;
    }

    public void MoveForward(float amount)
    {
        // Calculate forward vector (ignoring Y component for level movement)
        var forwardLevel = _direction with { Y = 0 };
        if (Vector3.Dot(forwardLevel, forwardLevel) > 0.0001f) // Check if not too small
        {
            forwardLevel = Vector3.Normalize(forwardLevel);
        }

        // Move position and target
        Position += forwardLevel * amount * MoveSpeed;
        Target = Position + _direction;
    }

    public void MoveRight(float amount)
    {
        // Calculate right vector (ignoring Y component for level movement)
        var rightLevel = _right with { Y = 0 };
        if (Vector3.Dot(rightLevel, rightLevel) > 0.0001f) // Check if not too small
        {
            rightLevel = Vector3.Normalize(rightLevel);
        }

        // Move position and target
        Position += rightLevel * amount * MoveSpeed;
        Target = Position + _direction;
    }

    public void MoveUp(float amount)
    {
        // Move directly up in world space
        var upVector = Vector3.UnitY;
        Position += upVector * amount * MoveSpeed;
        Target = Position + _direction;
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Target, Up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
    }
}