using System.Numerics;

namespace SearchitLibrary.Graphics;

public static class Helpers
{
    // Color mapping based on the colors in the Test.txt file
    private static readonly Dictionary<int, byte> _colorMap = new()
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

    // Converts an index back to coordinates
    public static Vector3 GetPosition(int index)
    {
        // Match the updated GetIndex formula by reversing it
        var x = index % Constants.ChunkSize;
        index /= Constants.ChunkSize;
        var y = index % Constants.ChunkSize;
        index /= Constants.ChunkSize;
        var z = index;

        return new Vector3(x, y, z);
    }

    public static void Foreach(Vector3 size, Action<Vector3> callback)
    {
        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
            callback(new Vector3(x, y, z));
    }

    public static void Foreach3(int size, Action<Vector3> callback)
    {
        for (var x = 0; x < size; x++)
        for (var y = 0; y < size; y++)
        for (var z = 0; z < size; z++)
            callback(new Vector3(x, y, z));
    }

    public static void Foreach2(int size, Action<Vector2> callback)
    {
        for (var x = 0; x < size; x++)
        for (var y = 0; y < size; y++)
            callback(new Vector2(x, y));
    }

    public static byte MapColorToVoxelType(this int color)
    {
        // Try to get the exact color match
        if (_colorMap.TryGetValue(color, out var voxelType)) return voxelType;

        // If no exact match, find the closest color
        var closestColor = 0xFFFFFF;
        var minDistance = int.MaxValue;

        foreach (var mapColor in _colorMap.Keys)
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

        return _colorMap[closestColor];
    }
}