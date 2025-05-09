using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SearchitBreakher.Graphics;
using SearchitLibrary.Graphics;
using System.IO;

namespace SearchitBreakher;

public class BreakerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private MonoGameCamera _camera;
    private SpriteFont _font;
    
    // Add chunk management
    private VoxelChunkManager _chunkManager;
    private ChunkRendererManager _chunkRendererManager;
    private int _loadRadius = 3; // How many chunks to load around the player

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

        // Initialize chunk renderer manager
        _chunkRendererManager = new ChunkRendererManager(GraphicsDevice, _camera);

        // Center the mouse in the window
        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load font for displaying info
        try
        {
            _font = Content.Load<SpriteFont>("Font");
        }
        catch
        {
            // Font might not be available yet
        }
        
        // Initialize chunk manager with path to voxels folder
        string voxelsPath = Path.Combine(Content.RootDirectory, "voxels");
        _chunkManager = new VoxelChunkManager(voxelsPath);
        
        // Ensure the directory exists
        _chunkManager.EnsureChunkDirectoryExists();
        
        // Initial load of chunks around the starting position
        UpdateLoadedChunks();
        
    }
    
    private void UpdateLoadedChunks()
    {
        // Convert MonoGame Vector3 to System.Numerics.Vector3 for chunk manager
        System.Numerics.Vector3 playerPos = new(
            _camera.Position.X,
            _camera.Position.Y,
            _camera.Position.Z
        );
        
        // Update chunks around player
        _chunkManager.UpdateChunksAroundPlayer(playerPos, _loadRadius);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Update camera with mouse input
        _camera.Update(GraphicsDevice, gameTime);

        // Update chunk renderer manager with the updated camera
        _chunkRendererManager.UpdateCamera(_camera);
        
        // Update which chunks are loaded based on player position
        UpdateLoadedChunks();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear both the color and depth buffer for proper 3D rendering
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

        // Enable depth buffer for 3D rendering
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        
        // Create a frustum for view-frustum culling
        BoundingFrustum viewFrustum = new BoundingFrustum(_camera.GetViewMatrix() * _camera.GetProjectionMatrix());
        
        // Draw all chunks in view frustum
        _chunkRendererManager.RenderVisibleChunks(_chunkManager, viewFrustum);

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
                
            // Add camera position info for debugging shading
            _spriteBatch.DrawString(_font, 
                $"Camera Position: ({_camera.Position.X:F2}, {_camera.Position.Y:F2}, {_camera.Position.Z:F2})",
                new Vector2(10, 70), Color.Yellow);
                
            // Add distance info
            float distance = Vector3.Distance(_camera.Position, Vector3.Zero);
            _spriteBatch.DrawString(_font, 
                $"Distance to Origin: {distance:F2} units",
                new Vector2(10, 90), Color.Yellow);
                
            // Add chunk loading info
            _spriteBatch.DrawString(_font, 
                $"Loaded Chunks: {_chunkManager.LoadedChunkCount} | Rendered: {_chunkRendererManager.RendererCount}",
                new Vector2(10, 110), Color.LightGreen);
            
            // Add face color info
            _spriteBatch.DrawString(_font, 
                "Front: Red | Back: Green | Left: Blue | Right: Yellow | Top: Magenta | Bottom: Cyan",
                new Vector2(10, 130), Color.White);

            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}