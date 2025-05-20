using System.Numerics;
using SearchitLibrary.Abstractions;

namespace SearchitLibrary.Graphics;

/// <summary>
/// Manages multiple voxel chunks, handles loading chunks, and provides access to voxels across chunk boundaries.
/// </summary>
public class VoxelChunkManager : IChunkManager
{
    private readonly string _chunkDirectory;
    private readonly Dictionary<Vector3, VoxelChunk?> _chunks;

    public VoxelChunkManager(string? chunkDirectory)
    {
        _chunks = new Dictionary<Vector3, VoxelChunk?>();
        _chunkDirectory = chunkDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "Chunks");

        // Create the directory if it doesn't exist
        if (!string.IsNullOrEmpty(_chunkDirectory) && !Directory.Exists(_chunkDirectory))
        {
            Directory.CreateDirectory(_chunkDirectory);
        }
    }

    public IEnumerable<VoxelChunk?> GetVisibleChunks()
    {
        return _chunks.Values;
    }

    /// <summary>
    /// Loads a chunk at the specified position. If the chunk doesn't exist, 
    /// returns null - it does NOT create a new chunk with test pattern.
    /// </summary>
    public VoxelChunk? LoadChunk(Vector3 chunkPosition)
    {
        // Check if the chunk is already loaded
        if (_chunks.TryGetValue(chunkPosition, out VoxelChunk? existingChunk))
        {
            return existingChunk;
        }

        VoxelChunk? chunk = null;

        // Determine the chunk file path
        string chunkFileName = $"Chunk_{chunkPosition.X}_{chunkPosition.Y}_{chunkPosition.Z}.gox";
        string chunkFilePath = Path.Combine(_chunkDirectory, chunkFileName);

        // Try to load the chunk from file if it exists
        if (!File.Exists(chunkFilePath)) return chunk;

        try
        {
            chunk = ChunkLoader.LoadGoxFile(chunkFilePath, chunkPosition);
            _chunks[chunkPosition] = chunk;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading chunk: {ex.Message}");
        }

        return chunk;
    }

    /// <summary>
    ///     Loads chunks in a radius around the player position and unloads distant chunks.
    /// </summary>
    public void UpdateChunksAroundPlayer(Vector3 playerPosition, int loadRadius)
    {
        // Identify chunks that should be kept loaded
        var chunksToKeep = IdentifyRelevantChunks(playerPosition, loadRadius);

        // Load chunks that aren't already loaded
        LoadNeededChunks(chunksToKeep);

        // Unload chunks that are too far away
        UnloadDistantChunks(chunksToKeep);
    }


    /// <summary>
    ///     Gets all currently loaded chunks.
    /// </summary>
    public IEnumerable<VoxelChunk> GetLoadedChunks()
    {
        return _chunks.Values.Where(chunk => chunk != null)!;
    }

    /// <summary>
    ///     Gets the number of currently loaded chunks.
    /// </summary>
    public int LoadedChunkCount => _chunks.Count;

    /// <summary>
    /// Unloads a chunk at the specified position.
    /// </summary>
    public void LoadChunk(Vector3 chunkPosition, VoxelChunk chunk)
    {
        _chunks.Add(chunkPosition, chunk);
    }

    /// <summary>
    /// Unloads a chunk at the specified position.
    /// </summary>
    public void UnloadChunk(Vector3 chunkPosition)
    {
        _chunks.Remove(chunkPosition);
    }

    /// <summary>
    /// Calculate the chunk position from a world position
    /// </summary>
    private Vector3 CalculateChunkPosition(Vector3 worldPosition)
    {
        return new Vector3(
            MathF.Floor(worldPosition.X / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(worldPosition.Y / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(worldPosition.Z / Constants.ChunkSize) * Constants.ChunkSize
        );
    }

    /// <summary>
    /// Converts a world position to chunk grid coordinates (integer indices)
    /// </summary>
    private static Vector3 GetChunkGridCoordinates(Vector3 worldPosition)
    {
        return new Vector3(
            (int)MathF.Floor(worldPosition.X / Constants.ChunkSize),
            (int)MathF.Floor(worldPosition.Y / Constants.ChunkSize),
            (int)MathF.Floor(worldPosition.Z / Constants.ChunkSize)
        );
    }

    /// <summary>
    /// Identifies chunks (by their world‐space origin) that should be kept loaded
    /// based on the player’s position and load radius.
    /// </summary>
    private HashSet<Vector3> IdentifyRelevantChunks(Vector3 playerPosition, int loadRadius)
    {
        var chunksToKeep = new HashSet<Vector3>();

        int chunkRadius = Math.Max(1, loadRadius);
        var span = chunkRadius * 2 + 1;
        var playerChunkOrigin = CalculateChunkPosition(playerPosition);

        // offset will run from (0,0,0) to (span-1, span-1, span-1)
        Helpers.Foreach3(span, offset =>
        {
            // recenter around zero and scale by ChunkSize
            var worldOffset = new Vector3(
                (offset.X - chunkRadius) * Constants.ChunkSize,
                (offset.Y - chunkRadius) * Constants.ChunkSize,
                (offset.Z - chunkRadius) * Constants.ChunkSize
            );

            chunksToKeep.Add(playerChunkOrigin + worldOffset);
        });

        return chunksToKeep;
    }


    /// <summary>
    /// Loads chunks that should be kept but aren't already loaded
    /// </summary>
    private void LoadNeededChunks(HashSet<Vector3> chunksToKeep)
    {
        foreach (var chunkPos in chunksToKeep
                     .Where(chunkPos => !_chunks.ContainsKey(chunkPos))
                     .Select(chunkPos => new { chunkPos, loadedChunk = LoadChunk(chunkPos) })
                     .Where(@t => @t.loadedChunk == null)
                     .Select(@t => @t.chunkPos))
        {
            _chunks[chunkPos] = null;
        }
    }

    /// <summary>
    /// Identifies and unloads chunks that are too far from the player
    /// </summary>
    private void UnloadDistantChunks(HashSet<Vector3> chunksToKeep)
    {
        var chunksToUnload = _chunks.Keys
            .Where(chunkPos => !chunksToKeep.Contains(chunkPos))
            .ToList();

        // Unload the identified chunks
        foreach (var chunkPos in chunksToUnload)
        {
            UnloadChunk(chunkPos);
        }
    }

    /// <summary>
    /// Gets the voxel at the specified world position, handling chunk boundaries.
    /// </summary>
    public byte GetVoxelAt(Vector3 worldPosition)
    {
        // Calculate the chunk position that contains this world position
        Vector3 chunkPos = CalculateChunkPosition(worldPosition);

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

        Vector3 chunkPos = CalculateChunkPosition(worldPosition);

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
    /// Gets the chunk that contains the specified world position.
    /// </summary>
    public VoxelChunk? GetChunkAt(Vector3 worldPosition)
    {
        // Calculate the chunk position
        var chunkPos = CalculateChunkPosition(worldPosition);

        // Try to get the chunk, or load it if it's not loaded
        if (!_chunks.TryGetValue(chunkPos, out VoxelChunk? chunk))
        {
            chunk = LoadChunk(chunkPos);
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
}