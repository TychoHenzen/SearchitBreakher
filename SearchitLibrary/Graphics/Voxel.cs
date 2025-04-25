using System.Numerics;

namespace SearchitLibrary.Graphics;

public class Voxel
{
    public Vector3 Position { get; }
    public Vector3 Color { get; }
    public float Size { get; }

    public Voxel(Vector3 position, Vector3 color, float size)
    {
        Position = position;
        Color = color;
        Size = size;
    }

    public static Voxel CreateBasicVoxel()
    {
        return new Voxel(
            new Vector3(0.0f, 0.0f, 0.0f),  // Origin
            new Vector3(1.0f, 0.0f, 0.0f),  // Red
            1.0f                            // Unit size
        );
    }

    public Vector3[] GetVertices()
    {
        float halfSize = Size / 2.0f;

        // Define 8 vertices for a cube centered at the position
        return new Vector3[]
        {
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // 0: front bottom left
            Position + new Vector3(halfSize, -halfSize, -halfSize),  // 1: front bottom right
            Position + new Vector3(-halfSize, halfSize, -halfSize),  // 2: front top left
            Position + new Vector3(halfSize, halfSize, -halfSize),   // 3: front top right
            Position + new Vector3(-halfSize, -halfSize, halfSize),  // 4: back bottom left
            Position + new Vector3(halfSize, -halfSize, halfSize),   // 5: back bottom right
            Position + new Vector3(-halfSize, halfSize, halfSize),   // 6: back top left
            Position + new Vector3(halfSize, halfSize, halfSize)     // 7: back top right
        };
    }

    public int[] GetIndices()
    {
        // Define triangles for a cube using indices (two triangles per face)
        return new int[]
        {
            // Front face
            0, 2, 1, // Triangle 1
            1, 2, 3, // Triangle 2
            
            // Back face
            5, 7, 4, // Triangle 3
            4, 7, 6, // Triangle 4
            
            // Left face
            4, 6, 0, // Triangle 5
            0, 6, 2, // Triangle 6
            
            // Right face
            1, 3, 5, // Triangle 7
            5, 3, 7, // Triangle 8
            
            // Top face
            2, 6, 3, // Triangle 9
            3, 6, 7, // Triangle 10
            
            // Bottom face
            0, 1, 4, // Triangle 11
            4, 1, 5  // Triangle 12
        };
    }
}