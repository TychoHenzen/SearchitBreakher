// /*
//  * Copyright 2025 Your Company Name
//  *
//  * Licensed under the appropriate license
//  */

using System.Numerics;
using SearchitLibrary;
using SearchitLibrary.Graphics;

namespace SearchitTest;

public static class TestHelpers
{
    /// <summary>
    ///     Creates test chunks in a cubic radius from the origin.
    /// </summary>
    public static void CreateTestChunks(this VoxelChunkManager mgr, int radius)
    {
        // Create test chunks in a (2*radius+1)^3 cube centered at the origin
        for (var x = -radius; x <= radius; x++)
        for (var y = -radius; y <= radius; y++)
        for (var z = -radius; z <= radius; z++)
        {
            Vector3 chunkPos = new(
                x * Constants.ChunkSize,
                y * Constants.ChunkSize,
                z * Constants.ChunkSize
            );

            // Load or create the chunk if it doesn't exist
            if (!mgr.IsChunkLoaded(chunkPos)) mgr.AddChunk(chunkPos, CreateTestChunk(chunkPos));
        }
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

    // Creates a test chunk with a pattern for debugging
    public static VoxelChunk CreateTestChunk(Vector3 position)
    {
        VoxelChunk chunk = new(position);

        // Create a simple pattern - a hollow box in the center
        int centerStart = Constants.ChunkSize / 4;
        int centerEnd = Constants.ChunkSize - centerStart;

        for (int x = centerStart; x < centerEnd; x++)
        {
            for (int z = centerStart; z < centerEnd; z++)
            {
                for (int y = centerStart; y < centerEnd; y++)
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

                        chunk.SetVoxel(x, y, z, value);
                    }
                }
            }
        }

        return chunk;
    }

    public static void CreateTestPattern(this byte[] voxelData)
    {
        // Create a simple test pattern - a box in the center
        var centerStart = Constants.ChunkSize / 4;
        var centerEnd = Constants.ChunkSize - centerStart;

        // Clear the array first to ensure we start with a clean slate
        for (var i = 0; i < voxelData.Length; i++) voxelData[i] = 0;

        // Iterate through coordinates to create a box pattern
        for (var x = 0; x < Constants.ChunkSize; x++)
        for (var z = 0; z < Constants.ChunkSize; z++)
        for (var y = 0; y < Constants.ChunkSize; y++)
        {
            // Calculate index using the reference formula: z * ChunkSize * ChunkSize + y * ChunkSize + x
            var index = Helpers.GetIndex(x, y, z);

            // Check if this voxel is part of our test pattern (a hollow box)
            if (x >= centerStart && x < centerEnd &&
                y >= centerStart && y < centerEnd &&
                z >= centerStart && z < centerEnd)
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