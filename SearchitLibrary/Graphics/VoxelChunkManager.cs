using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SearchitLibrary.Graphics;

/// <summary>
/// Manages multiple voxel chunks, handles loading chunks, and provides access to voxels across chunk boundaries.
/// </summary>
public class VoxelChunkManager
{
    private readonly Dictionary<Vector3, VoxelChunk> _chunks;
    private readonly string _chunkDirectory;
    private Vector3 _playerPosition;

    public VoxelChunkManager(string? chunkDirectory)
    {
        _chunks = new Dictionary<Vector3, VoxelChunk>();
        _chunkDirectory = chunkDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "Chunks");
        _playerPosition = Vector3.Zero;
        
        // Create the directory if it doesn't exist
        if (!string.IsNullOrEmpty(_chunkDirectory) && !Directory.Exists(_chunkDirectory))
        {
            Directory.CreateDirectory(_chunkDirectory);
        }
    }
    
    /// <summary>
    /// Loads a chunk at the specified position. If the chunk doesn't exist, 
    /// creates a new chunk with test pattern.
    /// </summary>
    public VoxelChunk? LoadChunk(Vector3 chunkPosition)
    {
        // Check if the chunk is already loaded
        if (_chunks.TryGetValue(chunkPosition, out VoxelChunk? existingChunk))
        {
            return existingChunk;
        }
        
        VoxelChunk? chunk;
        
        // Determine the chunk file path
        string chunkFileName = $"Chunk_{chunkPosition.X}_{chunkPosition.Y}_{chunkPosition.Z}.gox";
        string chunkFilePath = Path.Combine(_chunkDirectory, chunkFileName);
        
        // Try to load the chunk from file if it exists
        if (File.Exists(chunkFilePath))
        {
            try
            {
                chunk = ChunkLoader.LoadGoxFile(chunkFilePath, chunkPosition);
                _chunks[chunkPosition] = chunk;
                return chunk;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chunk: {ex.Message}");
            }
        
        }
        return null;
    }
    
    /// <summary>
    /// Unloads a chunk at the specified position.
    /// </summary>
    public void UnloadChunk(Vector3 chunkPosition)
    {
        _chunks.Remove(chunkPosition);
    }
    
    /// <summary>
    /// Loads chunks in a radius around the player position and unloads distant chunks.
    /// </summary>
    public void UpdateChunksAroundPlayer(Vector3 playerPosition, int loadRadius)
    {
        _playerPosition = playerPosition;
        
        // Calculate the chunk position the player is in
        Vector3 playerChunkPos = new Vector3(
            MathF.Floor(playerPosition.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(playerPosition.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(playerPosition.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        
        // Set of chunks that should be loaded
        HashSet<Vector3> chunksToKeep = new HashSet<Vector3>();
        
        // Load chunks in a radius around the player
        int chunkRadius = Math.Max(1, loadRadius);
        
        // Convert player position to chunk coordinates (integers)
        int playerChunkX = (int)MathF.Floor(playerPosition.X / VoxelChunk.ChunkSize);
        int playerChunkY = (int)MathF.Floor(playerPosition.Y / VoxelChunk.ChunkSize);
        int playerChunkZ = (int)MathF.Floor(playerPosition.Z / VoxelChunk.ChunkSize);
        
        // Add all chunks within the radius to the set of chunks to keep
        for (int x = playerChunkX - chunkRadius; x <= playerChunkX + chunkRadius; x++)
        {
            for (int y = playerChunkY - chunkRadius; y <= playerChunkY + chunkRadius; y++)
            {
                for (int z = playerChunkZ - chunkRadius; z <= playerChunkZ + chunkRadius; z++)
                {
                    Vector3 chunkPos = new Vector3(
                        x * VoxelChunk.ChunkSize,
                        y * VoxelChunk.ChunkSize,
                        z * VoxelChunk.ChunkSize
                    );
                    
                    chunksToKeep.Add(chunkPos);
                    
                    // Load the chunk if it's not already loaded
                    if (!_chunks.ContainsKey(chunkPos))
                    {
                        LoadChunk(chunkPos);
                    }
                }
            }
        }
        
        // Identify chunks to unload (chunks that are too far from the player)
        List<Vector3> chunksToUnload = new List<Vector3>();
        foreach (var chunkPos in _chunks.Keys)
        {
            if (!chunksToKeep.Contains(chunkPos))
            {
                chunksToUnload.Add(chunkPos);
            }
        }
        
        // Unload the identified chunks
        foreach (var chunkPos in chunksToUnload)
        {
            UnloadChunk(chunkPos);
        }
    }
    
    /// <summary>
    /// Gets all currently loaded chunks.
    /// </summary>
    public IEnumerable<VoxelChunk> GetLoadedChunks()
    {
        return _chunks.Values;
    }
    
    /// <summary>
    /// Gets the voxel at the specified world position, handling chunk boundaries.
    /// </summary>
    public byte GetVoxelAt(Vector3 worldPosition)
    {
        // Calculate the chunk position that contains this world position
        Vector3 chunkPos = new Vector3(
            MathF.Floor(worldPosition.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(worldPosition.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(worldPosition.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        
        // Check if the chunk is loaded
        if (!_chunks.TryGetValue(chunkPos, out VoxelChunk? chunk) || chunk == null)
        {
            return 0; // Empty voxel for unloaded chunks
        }
        
        // Calculate the position within the chunk
        Vector3 localPos = worldPosition - chunkPos;
        
        // Get the voxel from the chunk
        return chunk.GetVoxel(localPos);
    }
    
    /// <summary>
    /// Sets the voxel at the specified world position, handling chunk boundaries.
    /// </summary>
    public void SetVoxelAt(Vector3 worldPosition, byte value)
    {
        // Calculate the chunk position that contains this world position
        Vector3 chunkPos = new(
            MathF.Floor(worldPosition.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(worldPosition.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(worldPosition.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        
        // Try to get the chunk, or load it if it's not loaded
        if (!_chunks.TryGetValue(chunkPos, out VoxelChunk? chunk))
        {
            chunk = LoadChunk(chunkPos);
        }
        
        // If the chunk is null, we can't set the voxel
        if (chunk == null)
        {
            return;
        }
        
        // Calculate the position within the chunk
        Vector3 localPos = worldPosition - chunkPos;
        
        // Set the voxel in the chunk
        chunk.SetVoxel(localPos, value);
    }
    
    /// <summary>
    /// Creates directory for chunks if it doesn't exist.
    /// </summary>
    public void EnsureChunkDirectoryExists()
    {
        if (!string.IsNullOrEmpty(_chunkDirectory) && !Directory.Exists(_chunkDirectory))
        {
            Directory.CreateDirectory(_chunkDirectory);
        }
    }
    
    /// <summary>
    /// Creates test chunks in a cubic radius from the origin.
    /// </summary>
    public void CreateTestChunks(int radius)
    {
        // Create test chunks in a (2*radius+1)^3 cube centered at the origin
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3 chunkPos = new(
                        x * VoxelChunk.ChunkSize,
                        y * VoxelChunk.ChunkSize,
                        z * VoxelChunk.ChunkSize
                    );
                    
                    // Load or create the chunk if it doesn't exist
                    if (!_chunks.ContainsKey(chunkPos))
                    {
                        _chunks[chunkPos] = VoxelChunk.CreateTestChunk(chunkPos);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the chunk that contains the specified world position.
    /// </summary>
    public VoxelChunk? GetChunkAt(Vector3 worldPosition)
    {
        // Calculate the chunk position
        Vector3 chunkPos = new Vector3(
            MathF.Floor(worldPosition.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(worldPosition.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(worldPosition.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        
        // Try to get the chunk, or load it if it's not loaded
        if (!_chunks.TryGetValue(chunkPos, out VoxelChunk? chunk))
        {
            chunk = LoadChunk(chunkPos);
        }
        
        return chunk;
    }
    
    /// <summary>
    /// Gets the chunk at the specified chunk position.
    /// </summary>
    public VoxelChunk? GetChunk(Vector3 chunkPosition)
    {
        // Try to get the chunk, or load it if it's not loaded
        if (!_chunks.TryGetValue(chunkPosition, out VoxelChunk? chunk))
        {
            chunk = LoadChunk(chunkPosition);
        }
        
        return chunk;
    }
    
    /// <summary>
    /// Checks if a chunk is loaded at the specified position.
    /// </summary>
    public bool IsChunkLoaded(Vector3 chunkPosition)
    {
        return _chunks.ContainsKey(chunkPosition);
    }
    
    /// <summary>
    /// Gets the number of currently loaded chunks.
    /// </summary>
    public int LoadedChunkCount => _chunks.Count;
}
