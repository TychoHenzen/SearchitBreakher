using System.Numerics;

namespace SearchitLibrary.Graphics;

public class Voxel
{
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

    public Vector3 Position { get; }
    public float Size { get; }

    // Colors for each face
    public Vector3 FrontColor { get; }
    public Vector3 BackColor { get; }
    public Vector3 LeftColor { get; }
    public Vector3 RightColor { get; }
    public Vector3 TopColor { get; }
    public Vector3 BottomColor { get; }

    public static Voxel CreateBasicVoxel()
    {
        return new Voxel(
            new Vector3(0.0f, 0.0f, 0.0f), // Origin
            1.0f, // Unit size
            new Vector3(1.0f, 0.0f, 0.0f), // Front: Red
            new Vector3(0.0f, 1.0f, 0.0f), // Back: Green
            new Vector3(0.0f, 0.0f, 1.0f), // Left: Blue
            new Vector3(1.0f, 1.0f, 0.0f), // Right: Yellow
            new Vector3(1.0f, 0.0f, 1.0f), // Top: Magenta
            new Vector3(0.0f, 1.0f, 1.0f) // Bottom: Cyan
        );
    }

    public Vector3[] GetVertices()
    {
        float halfSize = Size / 2.0f;

        // Define 8 vertices for a cube centered at the position
        return
        [
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // 0: front bottom left
            Position + new Vector3(halfSize, -halfSize, -halfSize), // 1: front bottom right
            Position + new Vector3(-halfSize, halfSize, -halfSize), // 2: front top left
            Position + new Vector3(halfSize, halfSize, -halfSize), // 3: front top right
            Position + new Vector3(-halfSize, -halfSize, halfSize), // 4: back bottom left
            Position + new Vector3(halfSize, -halfSize, halfSize), // 5: back bottom right
            Position + new Vector3(-halfSize, halfSize, halfSize), // 6: back top left
            Position + new Vector3(halfSize, halfSize, halfSize) // 7: back top right
        ];
    }

    // Get colors for each vertex based on which faces they belong to
    // This returns 24 colors (one for each vertex in each face)
    public Vector3[] GetVertexColors()
    {
        return
        [
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
        ];
    }

    // Get vertices for each face separately (for colored faces)
    // Ensuring that vertices are defined in a consistent counter-clockwise order when viewed from outside
    public Vector3[] GetVerticesPerFace()
    {
        float halfSize = Size / 2.0f;

        return
        [
            // Front face (-Z direction) - Counter-clockwise when viewed from front
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left (0)
            Position + new Vector3(halfSize, -halfSize, -halfSize), // front bottom right (1)
            Position + new Vector3(-halfSize, halfSize, -halfSize), // front top left (2)
            Position + new Vector3(halfSize, halfSize, -halfSize), // front top right (3)

            // Back face (+Z direction) - Counter-clockwise when viewed from back
            Position + new Vector3(halfSize, -halfSize, halfSize), // back bottom right (4)
            Position + new Vector3(-halfSize, -halfSize, halfSize), // back bottom left (5)
            Position + new Vector3(halfSize, halfSize, halfSize), // back top right (6)
            Position + new Vector3(-halfSize, halfSize, halfSize), // back top left (7)

            // Left face (-X direction) - Counter-clockwise when viewed from left
            Position + new Vector3(-halfSize, -halfSize, halfSize), // back bottom left (8)
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left (9)
            Position + new Vector3(-halfSize, halfSize, halfSize), // back top left (10)
            Position + new Vector3(-halfSize, halfSize, -halfSize), // front top left (11)

            // Right face (+X direction) - Counter-clockwise when viewed from right
            Position + new Vector3(halfSize, -halfSize, -halfSize), // front bottom right (12)
            Position + new Vector3(halfSize, -halfSize, halfSize), // back bottom right (13)
            Position + new Vector3(halfSize, halfSize, -halfSize), // front top right (14)
            Position + new Vector3(halfSize, halfSize, halfSize), // back top right (15)

            // Top face (+Y direction) - Counter-clockwise when viewed from top
            Position + new Vector3(halfSize, halfSize, -halfSize), // front top right (16)
            Position + new Vector3(-halfSize, halfSize, -halfSize), // front top left (17)
            Position + new Vector3(halfSize, halfSize, halfSize), // back top right (18)
            Position + new Vector3(-halfSize, halfSize, halfSize), // back top left (19)

            // Bottom face (-Y direction) - Counter-clockwise when viewed from bottom
            Position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left (20)
            Position + new Vector3(halfSize, -halfSize, -halfSize), // front bottom right (21)
            Position + new Vector3(-halfSize, -halfSize, halfSize), // back bottom left (22)
            Position + new Vector3(halfSize, -halfSize, halfSize) // back bottom right (23)
        ];
    }

    public int[] GetIndices()
    {
        // Define triangles for a cube using indices (two triangles per face)
        // For each face, the vertices are arranged in counter-clockwise order when viewed from outside
        return
        [
            // Front face (-Z direction) - CCW from outside
            0, 1, 2, // Triangle 1: bottom-left → bottom-right → top-left
            2, 1, 3, // Triangle 2: top-left → bottom-right → top-right

            // Back face (+Z direction) - CCW from outside
            4, 5, 6, // Triangle 3: bottom-right → bottom-left → top-right
            6, 5, 7, // Triangle 4: top-right → bottom-left → top-left

            // Left face (-X direction) - CCW from outside
            8, 9, 10, // Triangle 5: back-bottom → front-bottom → back-top
            10, 9, 11, // Triangle 6: back-top → front-bottom → front-top

            // Right face (+X direction) - CCW from outside
            12, 13, 14, // Triangle 7: front-bottom → back-bottom → front-top
            14, 13, 15, // Triangle 8: front-top → back-bottom → back-top

            // Top face (+Y direction) - CCW from outside
            16, 17, 18, // Triangle 9: front-right → front-left → back-right
            18, 17, 19, // Triangle 10: back-right → front-left → back-left

            // Bottom face (-Y direction) - CCW from outside
            20, 22, 21, // Triangle 11: front-left → back-left → front-right
            21, 22, 23 // Triangle 12: front-right → back-left → back-right
        ];
    }
}