// BreakerGame.cs (Updated to use ConstantProvider)

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitLibrary.Abstractions;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

namespace SearchitBreakher;

public class BreakerGame : Game
{
    private const int LoadRadius = 3;
    private readonly GraphicsDeviceManager _graphics;
    private readonly IServiceProvider _sp;
    private ICamera _camera;
    private IChunkManager _chunkManager;
    private IChunkRenderer _chunkRenderer;
    private IConstantProvider _constantProvider;

    private SpriteFont _font;
    private MouseState _previousMouseState;

    private SpriteBatch _spriteBatch;

    public BreakerGame(IServiceProvider sp)
    {
        _sp = sp;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }


    protected override void Initialize()
    {
        _constantProvider = _sp.GetRequiredService<IConstantProvider>();
        _camera = _sp.GetRequiredService<ICamera>();
        _chunkManager = _sp.GetRequiredService<IChunkManager>();
        _chunkRenderer = _sp.GetRequiredService<IChunkRenderer>();
        CenterMouse();
        _previousMouseState = Mouse.GetState();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        TryLoadFont();
        UpdateLoadedChunks();
    }

    private void TryLoadFont()
    {
        try
        {
            _font = Content.Load<SpriteFont>("Font");
        }
        catch
        {
            // ignored
        }
    }


    private void CenterMouse()
    {
        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
    }

    private void UpdateLoadedChunks()
    {
        var playerPos = new Vector3(
            _camera.Position.X,
            _camera.Position.Y,
            _camera.Position.Z);

        _chunkManager.UpdateChunksAroundPlayer(playerPos, LoadRadius);
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsExitRequested()) Exit();

        var currentMouseState = Mouse.GetState();
        Vector2 delta = new(
            currentMouseState.X - _previousMouseState.X,
            currentMouseState.Y - _previousMouseState.Y);
        if (IsActive && delta != Vector2.Zero)
        {
            _camera.ApplyMouseDelta(delta);
            CenterMouse();
            _previousMouseState = Mouse.GetState();
        }

        _camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        _chunkRenderer.UpdateCamera(_camera);
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

        var projection = _camera.GetViewMatrix() * _camera.GetProjectionMatrix();

        _chunkRenderer.RenderVisibleChunks(_chunkManager, projection);
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

        _spriteBatch.DrawString(_font, $"Look Speed: {_constantProvider.Get().LookSpeed:F2} ([/] to adjust)",
            new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Move Speed: {_constantProvider.Get().MoveSpeed:F2} (-/+ to adjust)",
            new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, "WASD: Move, Mouse: Look, Space/C: Up/Down", new Vector2(10, 50), Color.White);
        _spriteBatch.DrawString(_font,
            $"Camera Position: ({_camera.Position.X:F2}, {_camera.Position.Y:F2}, {_camera.Position.Z:F2})",
            new Vector2(10, 70), Color.Yellow);
        _spriteBatch.DrawString(_font,
            $"Distance to Origin: {Microsoft.Xna.Framework.Vector3.Distance(_camera.Position, Microsoft.Xna.Framework.Vector3.Zero):F2} units",
            new Vector2(10, 90), Color.Yellow);
        _spriteBatch.DrawString(_font,
            $"Loaded Chunks: {_chunkManager.LoadedChunkCount} | Rendered: {_chunkRenderer.RendererCount}",
            new Vector2(10, 110), Color.LightGreen);
        _spriteBatch.DrawString(_font,
            "Front: Red | Back: Green | Left: Blue | Right: Yellow | Top: Magenta | Bottom: Cyan", new Vector2(10, 130),
            Color.White);

        _spriteBatch.End();
    }
}