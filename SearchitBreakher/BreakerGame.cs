// BreakerGame.cs (Updated to use ConstantProvider)

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitBreakher.Graphics;
using SearchitLibrary.Graphics;
using SearchitLibrary.IO;
using SearchitLibrary.Abstractions;
using System.IO;

namespace SearchitBreakher;

public class BreakerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private MonoGameCamera _camera;
    private SpriteFont _font;

    private VoxelChunkManager _chunkManager;
    private ChunkRendererManager _chunkRendererManager;

    private const int LoadRadius = 3;
    private MouseState _previousMouseState;

    public BreakerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        var constantsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "constants.json");
        IConstantProvider jsonProvider = new JsonConstantProvider(constantsPath);
        IConstantProvider constantProvider = new CachingConstantProvider(jsonProvider);

        _camera = new MonoGameCamera(GraphicsDevice, constantProvider);
        _chunkRendererManager = new ChunkRendererManager(GraphicsDevice, _camera);
        CenterMouse();
        _previousMouseState = Mouse.GetState();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        TryLoadFont();
        InitializeChunkManagement();
        UpdateLoadedChunks();
    }

    private void TryLoadFont()
    {
        try { _font = Content.Load<SpriteFont>("Font"); } catch { }
    }

    private void InitializeChunkManagement()
    {
        var voxelsPath = Path.Combine(Content.RootDirectory, "voxels");
        _chunkManager = new VoxelChunkManager(voxelsPath);
        _chunkManager.EnsureChunkDirectoryExists();
    }

    private void CenterMouse()
    {
        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
    }

    private void UpdateLoadedChunks()
    {
        var playerPos = new System.Numerics.Vector3(
            _camera.Position.X,
            _camera.Position.Y,
            _camera.Position.Z);

        _chunkManager.UpdateChunksAroundPlayer(playerPos, LoadRadius);
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsExitRequested()) Exit();

        if (IsActive)
        {
            MouseState currentMouseState = Mouse.GetState();
            Vector2 delta = new(
                currentMouseState.X - _previousMouseState.X,
                currentMouseState.Y - _previousMouseState.Y);

            if (delta != Vector2.Zero)
            {
                _camera.ApplyMouseDelta(delta);
                CenterMouse();
                _previousMouseState = Mouse.GetState();
            }
        }

        _camera.Update(GraphicsDevice, gameTime);
        _chunkRendererManager.UpdateCamera(_camera);
        UpdateLoadedChunks();

        base.Update(gameTime);
    }

    private static bool IsExitRequested()
    {
        var gamePadBack = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
        var keyboardEscape = Keyboard.GetState().IsKeyDown(Keys.Escape);
        return gamePadBack || keyboardEscape;
    }

    protected override void Draw(GameTime gameTime)
    {
        ClearScreen();

        var frustum = new BoundingFrustum(
            _camera.GetViewMatrix() * _camera.GetProjectionMatrix());

        _chunkRendererManager.RenderVisibleChunks(_chunkManager, frustum);
        DrawUI();

        base.Draw(gameTime);
    }

    private void ClearScreen()
    {
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    private void DrawUI()
    {
        if (_font == null) return;

        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        _spriteBatch.Begin();

        _spriteBatch.DrawString(_font, $"Look Speed: {_camera.Constants.LookSpeed:F2} ([/] to adjust)", new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Move Speed: {_camera.Constants.MoveSpeed:F2} (-/+ to adjust)", new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, "WASD: Move, Mouse: Look, Space/C: Up/Down", new Vector2(10, 50), Color.White);
        _spriteBatch.DrawString(_font, $"Camera Position: ({_camera.Position.X:F2}, {_camera.Position.Y:F2}, {_camera.Position.Z:F2})", new Vector2(10, 70), Color.Yellow);
        _spriteBatch.DrawString(_font, $"Distance to Origin: {Vector3.Distance(_camera.Position, Vector3.Zero):F2} units", new Vector2(10, 90), Color.Yellow);
        _spriteBatch.DrawString(_font, $"Loaded Chunks: {_chunkManager.LoadedChunkCount} | Rendered: {_chunkRendererManager.RendererCount}", new Vector2(10, 110), Color.LightGreen);
        _spriteBatch.DrawString(_font, "Front: Red | Back: Green | Left: Blue | Right: Yellow | Top: Magenta | Bottom: Cyan", new Vector2(10, 130), Color.White);

        _spriteBatch.End();
    }
}
