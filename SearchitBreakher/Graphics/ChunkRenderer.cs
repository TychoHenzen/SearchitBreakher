using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Abstractions;
using SearchitLibrary.Graphics;
using Vector3 = System.Numerics.Vector3;

namespace SearchitBreakher.Graphics
{
    public class ChunkRenderer
    {
        private readonly BasicEffect _basicEffect;
        private readonly GraphicsDevice _graphicsDevice;
        private ICamera _camera;
        private VoxelChunk _currentChunk;
        private int[] _indices;
        private VertexPositionColor[] _vertices;

        public ChunkRenderer(GraphicsDevice graphicsDevice, ICamera camera)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;

            // Create the effect
            _basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                View = camera.GetViewMatrix(),
                Projection = camera.GetProjectionMatrix(),
                World = Matrix.Identity
            };

            // Initialize with empty arrays
            _vertices = [];
            _indices = [];
        }

        public void SetChunk(VoxelChunk chunk)
        {
            _currentChunk = chunk;
            UpdateMesh();
        }

        private void UpdateMesh()
        {
            if (_currentChunk == null)
            {
                _vertices = [];
                _indices = [];
                return;
            }

            // Convert camera position to System.Numerics.Vector3
            Vector3 cameraPosition = new(
                _camera.Position.X,
                _camera.Position.Y,
                _camera.Position.Z
            );

            // Use the library's MeshGenerator to generate mesh data
            var meshData = MeshGenerator.GenerateMeshForChunk(_currentChunk, cameraPosition);

            // Convert to MonoGame vertices
            _vertices = new VertexPositionColor[meshData.Positions.Length];
            for (var i = 0; i < meshData.Positions.Length; i++)
            {
                _vertices[i] = new VertexPositionColor(
                    new Vector3(
                        meshData.Positions[i].X,
                        meshData.Positions[i].Y,
                        meshData.Positions[i].Z
                    ),
                    new Color(
                        meshData.Colors[i].X,
                        meshData.Colors[i].Y,
                        meshData.Colors[i].Z
                    )
                );
            }

            // Copy the indices
            _indices = meshData.Indices;
        }

        public void UpdateCamera(ICamera camera)
        {
            _camera = camera;
            _basicEffect.View = camera.GetViewMatrix();
            _basicEffect.Projection = camera.GetProjectionMatrix();

            // Update mesh with new camera position
            if (_currentChunk != null)
            {
                UpdateMesh();
            }
        }

        public void Draw()
        {
            // Check if we have anything to draw
            if (_vertices.Length == 0 || _indices.Length == 0)
            {
                return;
            }

            // Set render state
            _graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace, // Enable standard backface culling
                FillMode = FillMode.Solid
            };

            // Ensure depth buffer is enabled for proper z-ordering
            _graphicsDevice.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferFunction = CompareFunction.LessEqual
            };

            // Apply the effect
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Draw the chunk using an index buffer
                _graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices,
                    0, // vertex buffer offset
                    _vertices.Length, // vertex count
                    _indices, // index buffer
                    0, // index buffer offset
                    _indices.Length / 3 // primitive count
                );
            }
        }
    }
}