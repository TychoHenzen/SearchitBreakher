using System.Numerics;

namespace SearchitLibrary.Graphics;

public static class Helpers
{
    // Color mapping based on the colors in the Test.txt file
    private static readonly Dictionary<int, byte> ColorMap = new()
    {
        { 0x33FF33, 1 }, // Green
        { 0xFFFF33, 2 }, // Yellow
        { 0xFF3333, 3 }, // Red
        { 0x333333, 4 }, // Dark Gray
        { 0x3333FF, 5 }, // Blue
        { 0x33FFFF, 6 }, // Cyan
        { 0xCCCCCC, 7 }, // Light Gray
        { 0xCC33CC, 8 }, // Magenta
        { 0xFFFFFF, 0 } // White (empty)
    };

    // Converts coordinates to an index in the data array
    public static int GetIndex(int x, int y, int z)
    {
        // Ensure coordinates are within bounds
        x = Math.Clamp(x, 0, Constants.ChunkSize - 1);
        y = Math.Clamp(y, 0, Constants.ChunkSize - 1);
        z = Math.Clamp(z, 0, Constants.ChunkSize - 1);

        // Using z-major ordering for MonoGame coordinate system
        return z * Constants.ChunkSize * Constants.ChunkSize + y * Constants.ChunkSize + x;
    }

    public static void Foreach3(int start, int end, Action<Vector3> callback)
    {
        for (var x = start; x < end; x++)
        for (var y = start; y < end; y++)
        for (var z = start; z < end; z++)
            callback(new Vector3(x, y, z));
    }

    public static void Foreach3(int end, Action<Vector3> callback)
    {
        Foreach3(0, end, callback);
    }

    /// <summary>
    ///     Calculate the chunk position from a world position
    /// </summary>
    public static Vector3 CalculateChunkPosition(Vector3 worldPosition)
    {
        return new Vector3(
            MathF.Floor(worldPosition.X / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(worldPosition.Y / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(worldPosition.Z / Constants.ChunkSize) * Constants.ChunkSize
        );
    }

    public static byte MapColorToVoxelType(this int color)
    {
        // Try to get the exact color match
        if (ColorMap.TryGetValue(color, out var voxelType)) return voxelType;

        // If no exact match, find the closest color
        var closestColor = 0xFFFFFF;
        var minDistance = int.MaxValue;

        foreach (var mapColor in ColorMap.Keys)
        {
            var r1 = (color >> 16) & 0xFF;
            var g1 = (color >> 8) & 0xFF;
            var b1 = color & 0xFF;

            var r2 = (mapColor >> 16) & 0xFF;
            var g2 = (mapColor >> 8) & 0xFF;
            var b2 = mapColor & 0xFF;

            // Simple RGB distance calculation
            var distance = Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);

            if (distance >= minDistance) continue;

            minDistance = distance;
            closestColor = mapColor;
        }

        return ColorMap[closestColor];
    }
}