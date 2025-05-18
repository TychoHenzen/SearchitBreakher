// MonoGameCamera.cs (Refactored Snippet)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitLibrary;
using SearchitLibrary.Abstractions;
using System;
using SearchitLibrary.Graphics;

namespace SearchitBreakher.Graphics;

public class MonoGameCamera
{
    private readonly Camera _camera;
    private KeyboardState _previousKeyboardState;
    private Vector2 _mouseSensitivity;
    private Constants _constants;
    private readonly IConstantProvider _constantProvider;

    public Constants Constants => _constants;

    public Vector3 Position => new(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);

    public MonoGameCamera(GraphicsDevice graphicsDevice, IConstantProvider constantProvider, float fieldOfView = MathHelper.PiOver4)
    {
        _constantProvider = constantProvider;
        _constants = _constantProvider.Get();
        _mouseSensitivity = new Vector2(_constants.LookSpeed, _constants.LookSpeed);

        var position = new System.Numerics.Vector3(5, 3, 8);
        var target = new System.Numerics.Vector3(0, 0, 0);
        var up = new System.Numerics.Vector3(0, 1, 0);
        var aspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height;

        _camera = new Camera(position, target, up, fieldOfView, aspectRatio, 0.1f, 100f);
        _camera.MoveSpeed = _constants.MoveSpeed;

        _previousKeyboardState = Keyboard.GetState();
    }

    public Matrix GetViewMatrix() => ToMatrix(_camera.GetViewMatrix());
    public Matrix GetProjectionMatrix() => ToMatrix(_camera.GetProjectionMatrix());

    private static Matrix ToMatrix(System.Numerics.Matrix4x4 m) =>
        new(m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44);

    public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
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

        if (currentKeyboardState.IsKeyDown(Keys.OemOpenBrackets) && !_previousKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
        {
            _constants.LookSpeed = Math.Max(0.01f, _constants.LookSpeed - 0.01f);
            changed = true;
        }
        if (currentKeyboardState.IsKeyDown(Keys.OemCloseBrackets) && !_previousKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
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
