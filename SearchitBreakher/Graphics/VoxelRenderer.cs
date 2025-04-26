using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;
using System;

namespace SearchitBreakher.Graphics;

public class VoxelRenderer
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BasicEffect _basicEffect;
    private VertexPositionColor[] _vertices;
    private int[] _indices;
    private MonoGameCamera _camera;
    private Voxel _currentVoxel;

    // Constants for distance-based shading
    private const float MinDistance = 2.0f;
    private const float MaxDistance = 20.0f;
    private const float MinShade = 0.5f;
    private const float MaxShade = 1.0f;

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

        // Initialize with a basic voxel
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

        // Verify that the vertex and color arrays have the same length
        if (voxelVertices.Length != voxelColors.Length)
        {
            throw new InvalidOperationException($"Vertex count ({voxelVertices.Length}) does not match color count ({voxelColors.Length})");
        }

        // Create the vertex array
        _vertices = new VertexPositionColor[voxelVertices.Length];
        for (int i = 0; i < voxelVertices.Length; i++)
        {
            // Apply distance-based shading
            Vector3 position = new Vector3(voxelVertices[i].X, voxelVertices[i].Y, voxelVertices[i].Z);
            Vector3 cameraPosition = new Vector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);
            float distance = Vector3.Distance(position, cameraPosition);
            
            // Calculate shade factor based on distance
            float shadeFactor = CalculateShadeFactor(distance);
            
            // Apply shade factor to the color
            Vector3 color = voxelColors[i] * shadeFactor;
            
            // Clamp color values to valid range
            color.X = Math.Clamp(color.X, 0.0f, 1.0f);
            color.Y = Math.Clamp(color.Y, 0.0f, 1.0f);
            color.Z = Math.Clamp(color.Z, 0.0f, 1.0f);
            
            _vertices[i] = new VertexPositionColor(
                position,
                new Color(color.X, color.Y, color.Z)
            );
        }

        // Store the indices - these should reference the vertices within our 0-23 range
        _indices = voxelIndices;
    }

    private float CalculateShadeFactor(float distance)
    {
        // Linear interpolation between MinShade and MaxShade based on distance
        if (distance <= MinDistance)
            return MaxShade;
        if (distance >= MaxDistance)
            return MinShade;
            
        float t = (distance - MinDistance) / (MaxDistance - MinDistance);
        return MaxShade - t * (MaxShade - MinShade);
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
