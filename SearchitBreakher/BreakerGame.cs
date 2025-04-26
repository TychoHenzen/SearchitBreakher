using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitBreakher.Graphics;

namespace SearchitBreakher;

public class BreakerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private MonoGameCamera _camera;
    private VoxelRenderer _voxelRenderer;
    private SpriteFont _font;

    public BreakerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        // Hide mouse cursor for FPS-like controls
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        // Initialize the 3D camera
        _camera = new MonoGameCamera(GraphicsDevice);

        // Initialize the voxel renderer
        _voxelRenderer = new VoxelRenderer(GraphicsDevice, _camera);

        // Center the mouse in the window
        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load font for displaying info
        // Note: You need to add a SpriteFont to your Content project
        try
        {
            _font = Content.Load<SpriteFont>("Font");
        }
        catch
        {
            // Font might not be available yet
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Update camera with mouse input
        _camera.Update(GraphicsDevice, gameTime);

        // Update voxel renderer with the updated camera
        _voxelRenderer.UpdateCamera(_camera);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear both the color and depth buffer for proper 3D rendering
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

        // Enable depth buffer for 3D rendering
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        
        // Draw the 3D voxel
        _voxelRenderer.Draw();

        // Reset depth state for 2D UI drawing
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        
        // Draw UI info
        if (_font != null)
        {
            _spriteBatch.Begin();

            // Draw information about controls and current settings using the camera's constants
            _spriteBatch.DrawString(_font, $"Look Speed: {_camera.Constants.LookSpeed:F2} ([/] to adjust)",
                new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, $"Move Speed: {_camera.Constants.MoveSpeed:F2} (-/+ to adjust)",
                new Vector2(10, 30), Color.White);
            _spriteBatch.DrawString(_font, "WASD: Move, Mouse: Look, Space/C: Up/Down",
                new Vector2(10, 50), Color.White);

            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}