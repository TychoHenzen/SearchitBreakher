// MonoGameCamera.cs (Refactored Snippet)

using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitLibrary;
using SearchitLibrary.Abstractions;
using SearchitLibrary.Graphics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace SearchitBreakher.Graphics;

public class MonoGameCamera : ICamera
{
    private readonly Camera _camera;
    private readonly IConstantProvider _constantProvider;
    private readonly Constants _constants;
    private Vector2 _mouseSensitivity;
    private KeyboardState _previousKeyboardState;

    public MonoGameCamera(GraphicsDevice graphicsDevice, IConstantProvider constantProvider,
        float fieldOfView = MathHelper.PiOver4)
    {
        _constantProvider = constantProvider;
        _constants = _constantProvider.Get();
        _mouseSensitivity = new Vector2(_constants.LookSpeed, _constants.LookSpeed);

        var position = new Vector3(5, 3, 8);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var aspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height;

        _camera = new Camera(position, target, up, fieldOfView, aspectRatio, 0.1f, 100f)
        {
            MoveSpeed = _constants.MoveSpeed
        };

        _previousKeyboardState = Keyboard.GetState();
    }

    public Vector3 Position => new(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);


    public Matrix4x4 GetViewMatrix()
    {
        return _camera.GetViewMatrix();
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return _camera.GetProjectionMatrix();
    }

    public void Update(float deltaTime)
    {
        UpdateKeyboardMovement(deltaTime);
    }

    public void ApplyMouseDelta(Vector2 delta)
    {
        _camera.Yaw -= delta.X * _mouseSensitivity.X;
        _camera.Pitch -= delta.Y * _mouseSensitivity.Y;
    }

    private void UpdateKeyboardMovement(float deltaTime)
    {
        KeyboardState currentKeyboardState = Keyboard.GetState();

        if (currentKeyboardState.IsKeyDown(Keys.W)) _camera.MoveForward(deltaTime);
        if (currentKeyboardState.IsKeyDown(Keys.S)) _camera.MoveForward(-deltaTime);
        if (currentKeyboardState.IsKeyDown(Keys.A)) _camera.MoveRight(-deltaTime);
        if (currentKeyboardState.IsKeyDown(Keys.D)) _camera.MoveRight(deltaTime);
        if (currentKeyboardState.IsKeyDown(Keys.Space)) _camera.MoveUp(deltaTime);
        if (currentKeyboardState.IsKeyDown(Keys.C)) _camera.MoveUp(-deltaTime);

        bool changed = false;

        if (currentKeyboardState.IsKeyDown(Keys.OemOpenBrackets) &&
            !_previousKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
        {
            _constants.LookSpeed = Math.Max(0.01f, _constants.LookSpeed - 0.01f);
            changed = true;
        }

        if (currentKeyboardState.IsKeyDown(Keys.OemCloseBrackets) &&
            !_previousKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
        {
            _constants.LookSpeed += 0.01f;
            changed = true;
        }

        if (currentKeyboardState.IsKeyDown(Keys.OemMinus) && !_previousKeyboardState.IsKeyDown(Keys.OemMinus))
        {
            _constants.MoveSpeed = Math.Max(0.05f, _constants.MoveSpeed - 0.05f);
            changed = true;
        }

        if (currentKeyboardState.IsKeyDown(Keys.OemPlus) && !_previousKeyboardState.IsKeyDown(Keys.OemPlus))
        {
            _constants.MoveSpeed += 0.05f;
            changed = true;
        }

        if (changed)
        {
            UpdateFromConstants();
            _constantProvider.Save(_constants);
        }

        _previousKeyboardState = currentKeyboardState;
    }

    private void UpdateFromConstants()
    {
        _mouseSensitivity = new Vector2(_constants.LookSpeed, _constants.LookSpeed);
        _camera.MoveSpeed = _constants.MoveSpeed;
    }
}