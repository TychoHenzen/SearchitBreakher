using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitLibrary.Abstractions;

public interface IChunkRenderer
{
    int RendererCount { get; }
    void RenderChunk(VoxelChunk chunk);
    void UpdateCamera(ICamera camera);
    void RenderVisibleChunks(IChunkManager chunkManager, Matrix4x4 frustum);
}