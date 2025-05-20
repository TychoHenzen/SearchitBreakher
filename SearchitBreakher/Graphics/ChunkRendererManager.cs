using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary;
using SearchitLibrary.Abstractions;
using SearchitLibrary.Graphics;
using Vector3 = System.Numerics.Vector3;

namespace SearchitBreakher.Graphics
{
    /// <summary>
    /// Manages rendering of multiple chunks.
    /// </summary>
    public class ChunkRendererManager : IChunkRenderer
    {
        private readonly ICamera _camera;
        private readonly Dictionary<Vector3, ChunkRenderer> _chunkRenderers;
        private readonly GraphicsDevice _graphicsDevice;

        public ChunkRendererManager(GraphicsDevice graphicsDevice, ICamera camera)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _chunkRenderers = new Dictionary<Vector3, ChunkRenderer>();
        }

        /// <summary>
        ///     Gets the number of chunk renderers currently active.
        /// </summary>
        public int RendererCount => _chunkRenderers.Count;


        /// <summary>
        /// Updates all chunk renderers with the current camera.
        /// </summary>
        public void UpdateCamera(ICamera camera)
        {
            foreach (var renderer in _chunkRenderers.Values)
            {
                renderer.UpdateCamera(camera);
            }
        }


        /// <summary>
        /// Renders a specific chunk.
        /// </summary>
        public void RenderChunk(VoxelChunk chunk)
        {
            // Convert chunk position to a key for the dictionary
            var chunkPos = chunk.Position;

            // Get or create a renderer for this chunk
            if (!_chunkRenderers.TryGetValue(chunkPos, out var renderer))
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
        public void RenderVisibleChunks(IChunkManager chunkManager, Matrix4x4 viewProjection)
        {
            var m = new Matrix(
                viewProjection.M11, viewProjection.M12, viewProjection.M13, viewProjection.M14,
                viewProjection.M21, viewProjection.M22, viewProjection.M23, viewProjection.M24,
                viewProjection.M31, viewProjection.M32, viewProjection.M33, viewProjection.M34,
                viewProjection.M41, viewProjection.M42, viewProjection.M43, viewProjection.M44);

            var viewFrustum = new BoundingFrustum(m);
            foreach (var chunk in chunkManager.GetLoadedChunks())
            {
                // Build the chunk’s AABB in MonoGame types:
                var min = new Vector3(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
                var max = min + new Vector3(Constants.ChunkSize);
                var box = new BoundingBox(min, max);

                // If the box isn’t completely outside the frustum, draw it:
                if (viewFrustum.Intersects(box))
                {
                    RenderChunk(chunk);
                }
            }
        }
    }
}