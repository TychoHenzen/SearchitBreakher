using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace SearchitBreakher.Graphics;

/// <summary>
/// Manages rendering of multiple chunks.
/// </summary>
public class ChunkRendererManager
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly MonoGameCamera _camera;
    private readonly Dictionary<System.Numerics.Vector3, ChunkRenderer> _chunkRenderers;
    
    public ChunkRendererManager(GraphicsDevice graphicsDevice, MonoGameCamera camera)
    {
        _graphicsDevice = graphicsDevice;
        _camera = camera;
        _chunkRenderers = new Dictionary<System.Numerics.Vector3, ChunkRenderer>();
    }
    
    /// <summary>
    /// Updates all chunk renderers with the current camera.
    /// </summary>
    public void UpdateCamera(MonoGameCamera camera)
    {
        foreach (var renderer in _chunkRenderers.Values)
        {
            renderer.UpdateCamera(camera);
        }
    }
    
    /// <summary>
    /// Renders all chunks that are currently loaded in the manager.
    /// </summary>
    public void RenderLoadedChunks(VoxelChunkManager chunkManager)
    {
        // Render all loaded chunks
        var loadedChunks = chunkManager.GetLoadedChunks();
        foreach (var chunk in loadedChunks)
        {
            RenderChunk(chunk);
        }
    }
    
    /// <summary>
    /// Renders a specific chunk.
    /// </summary>
    public void RenderChunk(VoxelChunk chunk)
    {
        // Convert chunk position to a key for the dictionary
        System.Numerics.Vector3 chunkPos = chunk.Position;
        
        // Get or create a renderer for this chunk
        if (!_chunkRenderers.TryGetValue(chunkPos, out ChunkRenderer renderer))
        {
            renderer = new ChunkRenderer(_graphicsDevice, _camera);
            _chunkRenderers[chunkPos] = renderer;
            renderer.SetChunk(chunk);
        }
        
        // Draw the chunk
        renderer.Draw();
    }
    
    /// <summary>
    /// Renders chunks in view frustum only.
    /// </summary>
    public void RenderVisibleChunks(VoxelChunkManager chunkManager, BoundingFrustum viewFrustum)
    {
        foreach (var chunk in chunkManager.GetLoadedChunks())
        {
            // Convert the chunk position to XNA Vector3
            Vector3 chunkPosXna = new Vector3(
                chunk.Position.X, 
                chunk.Position.Y, 
                chunk.Position.Z
            );
            
            // Create a bounding box for the chunk
            BoundingBox chunkBounds = new BoundingBox(
                chunkPosXna, 
                chunkPosXna + new Vector3(VoxelChunk.ChunkSize, VoxelChunk.ChunkSize, VoxelChunk.ChunkSize)
            );
            
            // Check if the chunk is in the view frustum
            if (viewFrustum.Intersects(chunkBounds))
            {
                RenderChunk(chunk);
            }
        }
    }
    
    /// <summary>
    /// Clears all chunk renderers.
    /// </summary>
    public void ClearRenderers()
    {
        _chunkRenderers.Clear();
    }
    
    /// <summary>
    /// Removes a renderer for a specific chunk.
    /// </summary>
    public void RemoveRenderer(System.Numerics.Vector3 chunkPosition)
    {
        if (_chunkRenderers.ContainsKey(chunkPosition))
        {
            _chunkRenderers.Remove(chunkPosition);
        }
    }
    
    /// <summary>
    /// Gets the number of chunk renderers currently active.
    /// </summary>
    public int RendererCount => _chunkRenderers.Count;
}
