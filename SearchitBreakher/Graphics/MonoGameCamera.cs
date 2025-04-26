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
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;
    private Vector2 _mouseSensitivity;
    private Constants _constants;
    private string _constantsFilePath;

    public Constants Constants => _constants;

    public Vector3 Position => new Vector3(
        _camera.Position.X,
        _camera.Position.Y,
        _camera.Position.Z
    );

    public MonoGameCamera(GraphicsDevice graphicsDevice, float fieldOfView = MathHelper.PiOver4)
    {
        var position = new System.Numerics.Vector3(0, 0, 5);
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
            100f
        );

        // Initialize input states
        _previousMouseState = Mouse.GetState();
        _previousKeyboardState = Keyboard.GetState();

        // Load constants
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
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        );
    }

    public Matrix GetProjectionMatrix()
    {
        var matrix = _camera.GetProjectionMatrix();
        return new Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        );
    }

    public void UpdateAspectRatio(GraphicsDevice graphicsDevice)
    {
        _camera.AspectRatio = (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height;
    }

    public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Reload constants in case they were changed externally
        RefreshConstants();

        // Handle mouse for camera rotation
        UpdateMouseLook(graphicsDevice);

        // Handle keyboard for camera movement
        UpdateKeyboardMovement(deltaTime);
    }

    private void RefreshConstants()
    {
        _constants = Constants.LoadFromFile(_constantsFilePath);
        UpdateFromConstants();
    }

    private void UpdateMouseLook(GraphicsDevice graphicsDevice)
    {
        // Get current mouse state
        MouseState currentMouseState = Mouse.GetState();

        // Calculate mouse delta
        int deltaX = currentMouseState.X - _previousMouseState.X;
        int deltaY = currentMouseState.Y - _previousMouseState.Y;

        // Only process mouse input if the window has focus and there's actual movement
        if (deltaX != 0 || deltaY != 0)
        {
            // Update camera rotation based on mouse movement
            // Note the negative X so moving mouse left moves view counterclockwise
            _camera.Yaw -= deltaX * _mouseSensitivity.X;
            _camera.Pitch -= deltaY * _mouseSensitivity.Y; // Reversed because Y increases downwards

            // Reset mouse to center of screen to allow for continuous rotation
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            currentMouseState = Mouse.GetState(); // Update to avoid jumps
        }

        // Store current mouse state for next frame
        _previousMouseState = currentMouseState;
    }

    private void UpdateKeyboardMovement(float deltaTime)
    {
        // Get current keyboard state
        KeyboardState currentKeyboardState = Keyboard.GetState();

        // WASD movement relative to camera direction
        // W - forward, S - backward
        if (currentKeyboardState.IsKeyDown(Keys.W))
        {
            _camera.MoveForward(deltaTime);
        }
        if (currentKeyboardState.IsKeyDown(Keys.S))
        {
            _camera.MoveForward(-deltaTime);
        }

        // A - left, D - right
        if (currentKeyboardState.IsKeyDown(Keys.A))
        {
            _camera.MoveRight(-deltaTime);
        }
        if (currentKeyboardState.IsKeyDown(Keys.D))
        {
            _camera.MoveRight(deltaTime);
        }

        // Optional: Space - up, C - down
        if (currentKeyboardState.IsKeyDown(Keys.Space))
        {
            _camera.MoveUp(deltaTime);
        }
        if (currentKeyboardState.IsKeyDown(Keys.C))
        {
            _camera.MoveUp(-deltaTime);
        }

        // Adjust look speed ([ and ] keys)
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

        // Adjust move speed (- and = keys)
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

        // Store current keyboard state for next frame
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