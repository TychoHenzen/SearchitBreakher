using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;
using System;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace SearchitBreakher.Graphics
{
    public class VoxelRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _basicEffect;
        private VertexPositionColor[] _vertices;
        private int[] _indices;
        private MonoGameCamera _camera;
        private Voxel _currentVoxel;
        
        public VoxelRenderer(GraphicsDevice graphicsDevice, MonoGameCamera camera)
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
            
            // We're now using the ChunkRendererManager for rendering voxels
            // This basic voxel will only be used if chunk loading doesn't work
            SetVoxel(Voxel.CreateBasicVoxel());
        }
        
        public void SetVoxel(Voxel voxel)
        {
            _currentVoxel = voxel;
            UpdateVertices();
        }
        
        private void UpdateVertices()
        {
            // Get the voxel vertices, colors, and indices
            var voxelVertices = _currentVoxel.GetVerticesPerFace();
            var voxelColors = _currentVoxel.GetVertexColors();
            var voxelIndices = _currentVoxel.GetIndices();
            
            // Convert camera position to System.Numerics.Vector3 for shader use
            System.Numerics.Vector3 cameraPosition = new(
                _camera.Position.X, 
                _camera.Position.Y, 
                _camera.Position.Z
            );
            
            // Apply shading to colors with VoxelShader
            VoxelShader shader = new();
            System.Numerics.Vector3[] shadedColors = new System.Numerics.Vector3[voxelColors.Length];
            
            for (int i = 0; i < voxelVertices.Length; i++)
            {
                shadedColors[i] = shader.ApplyDistanceShading(
                    voxelColors[i], 
                    voxelVertices[i], 
                    cameraPosition
                );
            }
            
            // Create the vertex array for MonoGame
            _vertices = new VertexPositionColor[voxelVertices.Length];
            for (int i = 0; i < voxelVertices.Length; i++)
            {
                _vertices[i] = new VertexPositionColor(
                    new Vector3(voxelVertices[i].X, voxelVertices[i].Y, voxelVertices[i].Z),
                    new Color(shadedColors[i].X, shadedColors[i].Y, shadedColors[i].Z)
                );
            }
            
            // Store the indices
            _indices = voxelIndices;
        }
        
        public void UpdateCamera(MonoGameCamera camera)
        {
            _camera = camera;
            _basicEffect.View = camera.GetViewMatrix();
            _basicEffect.Projection = camera.GetProjectionMatrix();
            
            // Update vertex colors based on new camera position
            if (_currentVoxel != null)
            {
                UpdateVertices();
            }
        }
        
        public void Draw()
        {
            // Rendering code remains the same...
            // Clear the depth buffer before drawing
            _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
            
            // Set render state
            _graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,  // Enable standard backface culling
                FillMode = FillMode.Solid
            };
            
            // Ensure depth buffer is enabled and configured correctly for proper z-ordering
            _graphicsDevice.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferFunction = CompareFunction.LessEqual
            };

            // Apply the effect
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Draw the voxel using an index buffer
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