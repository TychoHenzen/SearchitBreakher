using System;
using System.Numerics;

namespace SearchitLibrary.Graphics;

public class Camera
{
    private Vector3 _position;
    private Vector3 _target;
    private Vector3 _up;
    private float _fieldOfView;
    private float _aspectRatio;
    private float _nearPlane;
    private float _farPlane;
    private float _yaw;
    private float _pitch;
    private Vector3 _direction;
    private Vector3 _right;
    private float _moveSpeed = 5.0f; // Units per second, will be updated from constants

    public Camera(Vector3 position, Vector3 target, Vector3 up, float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
    {
        _position = position;
        _target = target;
        _up = up;
        _fieldOfView = fieldOfView;
        _aspectRatio = aspectRatio;
        _nearPlane = nearPlane;
        _farPlane = farPlane;
        
        // Initialize yaw and pitch from initial target position
        Vector3 initialDirection = Vector3.Normalize(target - position);
        _pitch = (float)Math.Asin(initialDirection.Y);
        _yaw = (float)Math.Atan2(initialDirection.X, initialDirection.Z);
        _direction = initialDirection;
        
        // Initialize right vector
        _right = Vector3.Normalize(Vector3.Cross(_direction, _up));
    }

    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector3 Target
    {
        get => _target;
        set => _target = value;
    }

    public Vector3 Up
    {
        get => _up;
        set => _up = value;
    }
    
    public Vector3 Direction => _direction;
    
    public Vector3 Right => _right;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    public float FieldOfView
    {
        get => _fieldOfView;
        set => _fieldOfView = value;
    }

    public float AspectRatio
    {
        get => _aspectRatio;
        set => _aspectRatio = value;
    }

    public float NearPlane
    {
        get => _nearPlane;
        set => _nearPlane = value;
    }

    public float FarPlane
    {
        get => _farPlane;
        set => _farPlane = value;
    }
    
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
        _right = Vector3.Normalize(Vector3.Cross(_direction, _up));
        
        // Update target position based on position and direction
        _target = _position + _direction;
    }
    
    public void MoveForward(float amount)
    {
        // Calculate forward vector (ignoring Y component for level movement)
        Vector3 forwardLevel = _direction with { Y = 0 };
        if (Vector3.Dot(forwardLevel, forwardLevel) > 0.0001f) // Check if not too small
        {
            forwardLevel = Vector3.Normalize(forwardLevel);
        }
        
        // Move position and target
        _position += forwardLevel * amount * _moveSpeed;
        _target = _position + _direction;
    }
    
    public void MoveRight(float amount)
    {
        // Calculate right vector (ignoring Y component for level movement)
        Vector3 rightLevel = new Vector3(_right.X, 0, _right.Z);
        if (Vector3.Dot(rightLevel, rightLevel) > 0.0001f) // Check if not too small
        {
            rightLevel = Vector3.Normalize(rightLevel);
        }
        
        // Move position and target
        _position += rightLevel * amount * _moveSpeed;
        _target = _position + _direction;
    }
    
    public void MoveUp(float amount)
    {
        // Move directly up in world space
        Vector3 upVector = new Vector3(0, 1, 0);
        _position += upVector * amount * _moveSpeed;
        _target = _position + _direction;
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(_position, _target, _up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
    }
}