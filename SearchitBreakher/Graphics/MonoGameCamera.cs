using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitLibrary;
using SearchitLibrary.Graphics;
using System;
using System.IO;

namespace SearchitBreakher.Graphics;
public class MonoGameCamera
{
    private readonly Camera _camera;
    private KeyboardState _previousKeyboardState;
    private Vector2 _mouseSensitivity;
    private Constants _constants;
    private readonly string _constantsFilePath;

    public Constants Constants => _constants;

    public Vector3 Position => new(
        _camera.Position.X,
        _camera.Position.Y,
        _camera.Position.Z);

    public MonoGameCamera(GraphicsDevice graphicsDevice, float fieldOfView = MathHelper.PiOver4)
    {
        var position = new System.Numerics.Vector3(5, 3, 8);
        var target = new System.Numerics.Vector3(0, 0, 0);
        var up = new System.Numerics.Vector3(0, 1, 0);
        var aspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height;

        _camera = new Camera(
            position,
            target,
            up,
            fieldOfView,
            aspectRatio,
            0.1f,
            100f);

        _previousKeyboardState = Keyboard.GetState();
        _constantsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "constants.json");
        _constants = Constants.LoadFromFile(_constantsFilePath);
        _mouseSensitivity = new Vector2(_constants.LookSpeed, _constants.LookSpeed);
        _camera.MoveSpeed = _constants.MoveSpeed;
    }

    public Matrix GetViewMatrix()
    {
        var matrix = _camera.GetViewMatrix();
        return new Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }

    public Matrix GetProjectionMatrix()
    {
        var matrix = _camera.GetProjectionMatrix();
        return new Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }

    public void UpdateAspectRatio(GraphicsDevice graphicsDevice)
    {
        _camera.AspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height;
    }

    public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        RefreshConstants();
        UpdateKeyboardMovement(deltaTime);
    }

    public void ApplyMouseDelta(Vector2 delta)
    {
        _camera.Yaw -= delta.X * _mouseSensitivity.X;
        _camera.Pitch -= delta.Y * _mouseSensitivity.Y;
    }

    private void RefreshConstants()
    {
        _constants = Constants.LoadFromFile(_constantsFilePath);
        UpdateFromConstants();
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

        if (currentKeyboardState.IsKeyDown(Keys.OemOpenBrackets) && !_previousKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
        {
            _constants.LookSpeed = Math.Max(0.01f, _constants.LookSpeed - 0.01f);
            UpdateFromConstants();
            SaveConstants();
        }
        if (currentKeyboardState.IsKeyDown(Keys.OemCloseBrackets) && !_previousKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
        {
            _constants.LookSpeed += 0.01f;
            UpdateFromConstants();
            SaveConstants();
        }
        if (currentKeyboardState.IsKeyDown(Keys.OemMinus) && !_previousKeyboardState.IsKeyDown(Keys.OemMinus))
        {
            _constants.MoveSpeed = Math.Max(0.05f, _constants.MoveSpeed - 0.05f);
            UpdateFromConstants();
            SaveConstants();
        }
        if (currentKeyboardState.IsKeyDown(Keys.OemPlus) && !_previousKeyboardState.IsKeyDown(Keys.OemPlus))
        {
            _constants.MoveSpeed += 0.05f;
            UpdateFromConstants();
            SaveConstants();
        }

        _previousKeyboardState = currentKeyboardState;
    }

    private void UpdateFromConstants()
    {
        _mouseSensitivity = new Vector2(_constants.LookSpeed, _constants.LookSpeed);
        _camera.MoveSpeed = _constants.MoveSpeed;
    }

    private void SaveConstants()
    {
        _constants.SaveToFile(_constantsFilePath);
    }
}