using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;

namespace SearchitBreakher.Graphics;

public class TriangleRenderer
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BasicEffect _basicEffect;
    private readonly VertexPositionColor[] _vertices;

    public TriangleRenderer(GraphicsDevice graphicsDevice, MonoGameCamera camera)
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

        // Create the RGB triangle
        var triangle = Triangle.CreateRGBTriangle();

        // Convert to MonoGame vertices
        _vertices = new VertexPositionColor[3];
        for (int i = 0; i < 3; i++)
        {
            _vertices[i] = new VertexPositionColor(
                new Vector3(triangle.Vertices[i].X, triangle.Vertices[i].Y, triangle.Vertices[i].Z),
                new Color(triangle.Colors[i].X, triangle.Colors[i].Y, triangle.Colors[i].Z)
            );
        }
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
            CullMode = CullMode.None
        };

        // Apply the effect
        foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, 1);
        }
    }
}