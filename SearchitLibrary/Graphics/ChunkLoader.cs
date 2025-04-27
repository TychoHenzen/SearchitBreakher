using System;
using System.IO;
using System.Numerics;

namespace SearchitLibrary.Graphics;

public class ChunkLoader
{
    // Loads a chunk from a .gox file
    // This is a simplified implementation based on the reference code
    public static VoxelChunk LoadGoxFile(string filePath, Vector3 position)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find .gox file", filePath);
        }
        
        try
        {
            // Read the entire file as bytes
            byte[] fileData = File.ReadAllBytes(filePath);
            
            // For the initial implementation, we'll create a simpler parser than the reference
            // We'll assume a basic binary format that contains 32x32x32 bytes
            byte[] voxelData = new byte[VoxelChunk.ChunkSize * VoxelChunk.ChunkSize * VoxelChunk.ChunkSize];
            
            // Parse the file data (simplified version)
            ParseSimpleGoxFormat(fileData, voxelData);
            
            // Create and return the chunk
            return new VoxelChunk(position, voxelData);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load .gox file: {ex.Message}", ex);
        }
    }
    
    private static void ParseSimpleGoxFormat(byte[] fileData, byte[] voxelData)
    {
        // Initialize all voxels to empty (0)
        for (int i = 0; i < voxelData.Length; i++)
        {
            voxelData[i] = 0;
        }
        
        // Basic parsing of .gox format
        // The actual format might be different, but this is a starting point
        // Assuming a simple format where the first few bytes are header
        // and the rest are voxel data
        
        // Skip header (first 8 bytes as an example)
        int headerSize = Math.Min(8, fileData.Length);
        
        // Check if we have data after the header
        if (fileData.Length <= headerSize)
        {
            // Not enough data, fall back to test pattern
            CreateTestPattern(voxelData);
            return;
        }
        
        // Fix: Directly load the voxel data based on x,y,z coordinates
        int dataSize = Math.Min(voxelData.Length, fileData.Length - headerSize);
        int chunkSize = VoxelChunk.ChunkSize;
        
        // The proper approach is to map the data from the file to the correct 3D position
        // based on the .gox file format
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    // Calculate file index based on position
                    // Assuming the file format stores data in x,y,z order
                    int fileIndex = x + (y * chunkSize) + (z * chunkSize * chunkSize);
                    
                    // Skip if beyond the available data
                    if (headerSize + fileIndex >= fileData.Length)
                        continue;
                        
                    // Calculate the destination index in our voxel data array using the same formula as fileIndex
                    // This ensures consistency and fixes the voxel data loading
                    int destIndex = fileIndex;
                    
                    // Ensure we don't exceed array bounds
                    if (destIndex >= 0 && destIndex < voxelData.Length && 
                        headerSize + fileIndex < fileData.Length)
                    {
                        voxelData[destIndex] = fileData[headerSize + fileIndex];
                        
                        // Process the voxel value
                        if (voxelData[destIndex] > 0)
                        {
                            voxelData[destIndex] = (byte)(voxelData[destIndex] % 7); // Ensure we use only our defined color types (1-6)
                            if (voxelData[destIndex] == 0) voxelData[destIndex] = 1; // Prevent zero values
                        }
                    }
                }
            }
        }
        
        // Check if we have any non-zero voxels after loading
        bool hasVoxels = false;
        for (int i = 0; i < voxelData.Length; i++)
        {
            if (voxelData[i] != 0)
            {
                hasVoxels = true;
                break;
            }
        }
        
        // If no voxels were loaded or the file didn't contain enough data, use a test pattern
        if (!hasVoxels)
        {
            CreateTestPattern(voxelData);
        }
    }
    
    private static void CreateTestPattern(byte[] voxelData)
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
            for (int y = 0; y < VoxelChunk.ChunkSize; y++)
            {
                for (int z = 0; z < VoxelChunk.ChunkSize; z++)
                {
                    // Calculate the index in the 1D array using the fixed GetIndex method
                    int index = VoxelChunk.GetIndex(x, y, z);
                    
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
    
    // In the future, we should implement more sophisticated loader methods:
    // - LoadGoxFile that properly reads the GOX format with sections
    // - Support for other voxel formats
}
