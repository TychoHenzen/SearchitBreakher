using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitLibrary.Abstractions;

public interface IChunkManager
{
    int LoadedChunkCount { get; }
    VoxelChunk? LoadChunk(Vector3 playerPosition);
    IEnumerable<VoxelChunk?> GetVisibleChunks();
    void UpdateChunksAroundPlayer(Vector3 playerPos, int loadRadius);
    IEnumerable<VoxelChunk> GetLoadedChunks();
}