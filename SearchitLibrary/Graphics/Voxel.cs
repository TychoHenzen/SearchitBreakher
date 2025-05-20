using System.Numerics;

namespace SearchitLibrary.Graphics;

public class Voxel(Vector3 position, float size)
{
    public Vector3 Position { get; } = position;
    public float Size { get; } = size;

    public static Voxel CreateBasicVoxel()
    {
        return new Voxel(
            new Vector3(0.0f, 0.0f, 0.0f), // Origin
            1.0f // Unit size
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

    public static int[] GetIndices()
    {
        // Define triangles for a cube using indices (two triangles per face)
        // For each face, the vertices are arranged in counter-clockwise order when viewed from outside
        return
        [
            // Front face (-Z direction) - CCW from outside
            0, 1, 2,
            2, 1, 3,

            // Back face (+Z direction) - CCW from outside
            4, 5, 6,
            6, 5, 7,

            // Left face (-X direction) - CCW from outside
            8, 9, 10,
            10, 9, 11,

            // Right face (+X direction) - CCW from outside
            12, 13, 14,
            14, 13, 15,

            // Top face (+Y direction) - CCW from outside
            16, 17, 18,
            18, 17, 19,

            // Bottom face (-Y direction) - CCW from outside
            20, 22, 21,
            21, 22, 23
        ];
    }
}