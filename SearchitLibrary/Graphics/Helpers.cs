namespace SearchitLibrary.Graphics;

using System.Numerics;

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
		{ 0xFFFFFF, 0 }  // White (empty)
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
		int x = index % Constants.ChunkSize;
		index /= Constants.ChunkSize;
		int y = index % Constants.ChunkSize;
		index /= Constants.ChunkSize;
		int z = index;
		
		return new Vector3(x, y, z);
	}
	public static byte MapColorToVoxelType(this int color)
	{
		// Try to get the exact color match
		if (_colorMap.TryGetValue(color, out byte voxelType))
		{
			return voxelType;
		}
		
		// If no exact match, find the closest color
		int closestColor = 0xFFFFFF;
		int minDistance = int.MaxValue;
		
		foreach (int mapColor in _colorMap.Keys)
		{
			int r1 = (color >> 16) & 0xFF;
			int g1 = (color >> 8) & 0xFF;
			int b1 = color & 0xFF;
			
			int r2 = (mapColor >> 16) & 0xFF;
			int g2 = (mapColor >> 8) & 0xFF;
			int b2 = mapColor & 0xFF;
			
			// Simple RGB distance calculation
			int distance = Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
			
			if (distance < minDistance)
			{
				minDistance = distance;
				closestColor = mapColor;
			}
		}
		
		return _colorMap[closestColor];
	}
}