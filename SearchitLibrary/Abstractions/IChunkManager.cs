using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitLibrary.Abstractions;

public interface IChunkManager
{
    int LoadedChunkCount { get; }
    void UpdateChunksAroundPlayer(Vector3 playerPos, int loadRadius);
    IEnumerable<VoxelChunk> GetLoadedChunks();
}