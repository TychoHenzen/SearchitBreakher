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
		{ 0xFFFFFF, 0 }  // White (empty)
	};
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
	public static void CreateTestPattern(this byte[] voxelData)
	{
		// Create a simple test pattern - a box in the center
		int centerStart = VoxelChunk.ChunkSize / 4;
		int centerEnd = VoxelChunk.ChunkSize - centerStart;
		
		// Clear the array first to ensure we start with a clean slate
		for (int i = 0; i < voxelData.Length; i++)
		{
			voxelData[i] = 0;
		}
		
		// Iterate through coordinates to create a box pattern
		for (int x = 0; x < VoxelChunk.ChunkSize; x++)
		{
			for (int z = 0; z < VoxelChunk.ChunkSize; z++)
			{
				for (int y = 0; y < VoxelChunk.ChunkSize; y++)
				{
					// Calculate index using the reference formula: z * ChunkSize * ChunkSize + y * ChunkSize + x
					int index = z * VoxelChunk.ChunkSize * VoxelChunk.ChunkSize +
					            y * VoxelChunk.ChunkSize +
					            x;
					
					// Check if this voxel is part of our test pattern (a hollow box)
					if (x >= centerStart && x < centerEnd &&
					    y >= centerStart && y < centerEnd &&
					    z >= centerStart && z < centerEnd)
					{
						// Set voxels in the outer shell only
						if (x == centerStart || x == centerEnd - 1 ||
						    y == centerStart || y == centerEnd - 1 ||
						    z == centerStart || z == centerEnd - 1)
						{
							// Use different colors for each face to make them distinguishable
							byte value = 1; // Default
							
							// Assign colors based on which face the voxel belongs to
							// Front face (negative Z)
							if (z == centerStart)
								value = 1; // Red
							// Back face (positive Z)
							else if (z == centerEnd - 1)
								value = 2; // Green
							// Left face (negative X)
							else if (x == centerStart)
								value = 3; // Blue
							// Right face (positive X)
							else if (x == centerEnd - 1)
								value = 4; // Yellow
							// Top face (positive Y)
							else if (y == centerEnd - 1)
								value = 5; // Cyan
							// Bottom face (negative Y)
							else if (y == centerStart)
								value = 6; // Magenta
							
							// Set the voxel value
							voxelData[index] = value;
						}
					}
				}
			}
		}
	}
}