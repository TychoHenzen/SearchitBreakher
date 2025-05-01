using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;
using System;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace SearchitBreakher.Graphics
{
    public class ChunkRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _basicEffect;
        private VertexPositionColor[] _vertices;
        private int[] _indices;
        private MonoGameCamera _camera;
        private VoxelChunk _currentChunk;
        
        public ChunkRenderer(GraphicsDevice graphicsDevice, MonoGameCamera camera)
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
            _vertices = Array.Empty<VertexPositionColor>();
            _indices = Array.Empty<int>();
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
                _vertices = Array.Empty<VertexPositionColor>();
                _indices = Array.Empty<int>();
                return;
            }
            
            // Convert camera position to System.Numerics.Vector3
            System.Numerics.Vector3 cameraPosition = new(
                _camera.Position.X, 
                _camera.Position.Y, 
                _camera.Position.Z
            );
            
            // Use the library's MeshGenerator to generate mesh data
            var meshData = MeshGenerator.GenerateMeshForChunk(_currentChunk, cameraPosition);
            
            // Convert to MonoGame vertices
            _vertices = new VertexPositionColor[meshData.Positions.Length];
            for (int i = 0; i < meshData.Positions.Length; i++)
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
        
        public void UpdateCamera(MonoGameCamera camera)
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
                CullMode = CullMode.CullCounterClockwiseFace,  // Enable standard backface culling
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
                    0,                  // vertex buffer offset
                    _vertices.Length,   // vertex count
                    _indices,           // index buffer
                    0,                  // index buffer offset
                    _indices.Length / 3 // primitive count
                );
            }
        }
    }
}