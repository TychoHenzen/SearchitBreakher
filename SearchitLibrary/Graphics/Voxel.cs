using System.Numerics;

namespace SearchitLibrary.Graphics;

public class Voxel
{
    public Vector3 Position { get; }
    public float Size { get; }
    
    // Colors for each face
    public Vector3 FrontColor { get; }
    public Vector3 BackColor { get; }
    public Vector3 LeftColor { get; }
    public Vector3 RightColor { get; }
    public Vector3 TopColor { get; }
    public Vector3 BottomColor { get; }

    public Voxel(
        Vector3 position, 
        float size,
        Vector3 frontColor,
        Vector3 backColor,
        Vector3 leftColor,
        Vector3 rightColor,
        Vector3 topColor,
        Vector3 bottomColor)
    {
        Position = position;
        Size = size;
        FrontColor = frontColor;
        BackColor = backColor;
        LeftColor = leftColor;
        RightColor = rightColor;
        TopColor = topColor;
        BottomColor = bottomColor;
    }

    // Constructor that takes a single color and applies it to all faces
    public Voxel(Vector3 position, Vector3 color, float size)
        : this(position, size, color, color, color, color, color, color)
    {
    }

    public static Voxel CreateBasicVoxel()
    {
        return new Voxel(
            new Vector3(0.0f, 0.0f, 0.0f),  // Origin
            1.0f,                           // Unit size
            new Vector3(1.0f, 0.0f, 0.0f),  // Front: Red
            new Vector3(0.0f, 1.0f, 0.0f),  // Back: Green
            new Vector3(0.0f, 0.0f, 1.0f),  // Left: Blue
            new Vector3(1.0f, 1.0f, 0.0f),  // Right: Yellow
            new Vector3(1.0f, 0.0f, 1.0f),  // Top: Magenta
            new Vector3(0.0f, 1.0f, 1.0f)   // Bottom: Cyan
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

    // Get colors for each vertex based on which faces they belong to
    // This returns 24 colors (one for each vertex in each face)
    public Vector3[] GetVertexColors()
    {
        return new Vector3[]
        {
            // Front face (4 vertices)
            FrontColor, FrontColor, FrontColor, FrontColor,
            
            // Back face (4 vertices)
            BackColor, BackColor, BackColor, BackColor,
            
            // Left face (4 vertices)
            LeftColor, LeftColor, LeftColor, LeftColor,
            
            // Right face (4 vertices)
            RightColor, RightColor, RightColor, RightColor,
            
            // Top face (4 vertices)
            TopColor, TopColor, TopColor, TopColor,
            
            // Bottom face (4 vertices)
            BottomColor, BottomColor, BottomColor, BottomColor
        };
    }

    // Get vertices for each face separately (for colored faces)
    public Vector3[] GetVerticesPerFace()
    {
        float halfSize = Size / 2.0f;
        
        return new Vector3[]
        {
            // Front face (4 vertices)
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
            Position + new Vector3(halfSize, -halfSize, -halfSize),  // front bottom right
            Position + new Vector3(-halfSize, halfSize, -halfSize),  // front top left
            Position + new Vector3(halfSize, halfSize, -halfSize),   // front top right
            
            // Back face (4 vertices)
            Position + new Vector3(-halfSize, -halfSize, halfSize),  // back bottom left
            Position + new Vector3(halfSize, -halfSize, halfSize),   // back bottom right
            Position + new Vector3(-halfSize, halfSize, halfSize),   // back top left
            Position + new Vector3(halfSize, halfSize, halfSize),    // back top right
            
            // Left face (4 vertices)
            Position + new Vector3(-halfSize, -halfSize, halfSize),  // back bottom left
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
            Position + new Vector3(-halfSize, halfSize, halfSize),   // back top left
            Position + new Vector3(-halfSize, halfSize, -halfSize),  // front top left
            
            // Right face (4 vertices)
            Position + new Vector3(halfSize, -halfSize, -halfSize),  // front bottom right
            Position + new Vector3(halfSize, -halfSize, halfSize),   // back bottom right
            Position + new Vector3(halfSize, halfSize, -halfSize),   // front top right
            Position + new Vector3(halfSize, halfSize, halfSize),    // back top right
            
            // Top face (4 vertices)
            Position + new Vector3(-halfSize, halfSize, -halfSize),  // front top left
            Position + new Vector3(halfSize, halfSize, -halfSize),   // front top right
            Position + new Vector3(-halfSize, halfSize, halfSize),   // back top left
            Position + new Vector3(halfSize, halfSize, halfSize),    // back top right
            
            // Bottom face (4 vertices)
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
            Position + new Vector3(halfSize, -halfSize, -halfSize),  // front bottom right
            Position + new Vector3(-halfSize, -halfSize, halfSize),  // back bottom left
            Position + new Vector3(halfSize, -halfSize, halfSize)    // back bottom right
        };
    }

    public int[] GetIndices()
    {
        // Define triangles for a cube using indices (two triangles per face)
        // Adjusted for the new vertex layout (24 vertices, 4 per face)
        return new int[]
        {
            // Front face
            0, 2, 1,  // Triangle 1
            1, 2, 3,  // Triangle 2
            
            // Back face
            4, 6, 5,  // Triangle 3
            5, 6, 7,  // Triangle 4
            
            // Left face
            8, 10, 9,  // Triangle 5
            9, 10, 11, // Triangle 6
            
            // Right face
            12, 14, 13, // Triangle 7
            13, 14, 15, // Triangle 8
            
            // Top face
            16, 18, 17, // Triangle 9
            17, 18, 19, // Triangle 10
            
            // Bottom face
            20, 21, 22, // Triangle 11
            22, 21, 23  // Triangle 12
        };
    }
}
