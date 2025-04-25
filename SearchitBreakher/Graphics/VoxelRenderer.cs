using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;

namespace SearchitBreakher.Graphics;

public class VoxelRenderer
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BasicEffect _basicEffect;
    private VertexPositionColor[] _vertices;
    private int[] _indices;

    public VoxelRenderer(GraphicsDevice graphicsDevice, MonoGameCamera camera)
    {
        _graphicsDevice = graphicsDevice;

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
        // Get the voxel vertices and indices
        var voxelVertices = voxel.GetVertices();
        var voxelIndices = voxel.GetIndices();

        // Create the vertex array
        _vertices = new VertexPositionColor[voxelVertices.Length];
        for (int i = 0; i < voxelVertices.Length; i++)
        {
            _vertices[i] = new VertexPositionColor(
                new Vector3(voxelVertices[i].X, voxelVertices[i].Y, voxelVertices[i].Z),
                new Color(voxel.Color.X, voxel.Color.Y, voxel.Color.Z)
            );
        }

        // Store the indices
        _indices = voxelIndices;
    }

    public void UpdateCamera(MonoGameCamera camera)
    {
        _basicEffect.View = camera.GetViewMatrix();
        _basicEffect.Projection = camera.GetProjectionMatrix();
    }

    public void Draw()
    {
        // Set render state
        _graphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace
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