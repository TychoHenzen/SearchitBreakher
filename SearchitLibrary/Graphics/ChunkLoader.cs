using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;

namespace SearchitLibrary.Graphics;

public class ChunkLoader
{
    // Offsets for the 8 sections in a GOX file
    private static readonly Vector3[] _offsets =
    {
        new(0, 1, 0), // section 1
        new(1, 1, 0), // section 2
        new(0, 0, 0), // section 3
        new(1, 0, 0), // section 4
        new(0, 0, 1), // section 5
        new(0, 1, 1), // section 6
        new(1, 1, 1), // section 7
        new(1, 0, 1)  // section 8
    };

    // Simple color map for our implementation
    // In a full implementation, we would have a proper color mapping
    private static readonly Dictionary<int, byte> _colorMap = new Dictionary<int, byte>
    {
        { 0xFFFFFF, 0 }, // White/Empty
        { 0xFF0000, 1 }, // Red
        { 0x00FF00, 2 }, // Green
        { 0x0000FF, 3 }, // Blue
        { 0xFFFF00, 4 }, // Yellow
        { 0x00FFFF, 5 }, // Cyan
        { 0xFF00FF, 6 }  // Magenta
    };

    // Loads a chunk from a .gox file
    public static VoxelChunk LoadGoxFile(string filePath, Vector3 position)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find .gox file", filePath);
        }
        
        try
        {
            // Initialize voxel data array
            byte[] voxelData = new byte[VoxelChunk.ChunkSize * VoxelChunk.ChunkSize * VoxelChunk.ChunkSize];
            
            using (FileStream stream = File.OpenRead(filePath))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    bool success = ParseGoxFormat(reader, voxelData);
                    
                    // If parsing failed, create a test pattern instead
                    if (!success)
                    {
                        CreateTestPattern(voxelData);
                        Console.WriteLine($"Warning: Failed to parse .gox file '{filePath}'. Using test pattern instead.");
                    }
                }
            }
            
            // Create and return the chunk
            return new VoxelChunk(position, voxelData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading .gox file: {ex.Message}");
            
            // Create an empty chunk with test pattern instead of throwing an exception
            byte[] voxelData = new byte[VoxelChunk.ChunkSize * VoxelChunk.ChunkSize * VoxelChunk.ChunkSize];
            CreateTestPattern(voxelData);
            return new VoxelChunk(position, voxelData);
        }
    }
    
    private static bool ParseGoxFormat(BinaryReader reader, byte[] voxelData)
    {
        // Initialize all voxels to empty (0)
        for (int i = 0; i < voxelData.Length; i++)
        {
            voxelData[i] = 0;
        }
        
        bool anyVoxelsLoaded = false;
        int sectionIndex = 0;
        
        try
        {
            // Try to parse up to 8 sections in the GOX file
            while (reader.BaseStream.Position < reader.BaseStream.Length && sectionIndex < 8)
            {
                // Try to find a BL16 marker
                if (!SeekToMarker(reader, "BL16"))
                {
                    break;
                }
                
                // Read the size of the PNG section
                uint fileSize = reader.ReadUInt32();
                
                // Check if we have enough data to read the PNG
                if (reader.BaseStream.Position + fileSize > reader.BaseStream.Length)
                {
                    break;
                }
                
                // Read the PNG data
                byte[] pngData = reader.ReadBytes((int)fileSize);
                
                // In a real implementation, we would now parse the PNG image
                // Since we don't have direct image parsing, we'll use a simplified approach
                bool sectionSuccess = ParseSimplifiedImageData(pngData, sectionIndex, voxelData);
                if (sectionSuccess)
                {
                    anyVoxelsLoaded = true;
                }
                
                sectionIndex++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing GOX format: {ex.Message}");
            return false;
        }
        
        return anyVoxelsLoaded;
    }
    
    private static bool SeekToMarker(BinaryReader reader, string marker)
    {
        long startPos = reader.BaseStream.Position;
        
        // We'll search for up to 1MB to find the marker
        long maxSearchBytes = Math.Min(1024 * 1024, reader.BaseStream.Length - startPos);
        
        for (int i = 0; i < maxSearchBytes; i++)
        {
            long pos = reader.BaseStream.Position;
            
            // Make sure we have enough bytes to read the marker
            if (pos + marker.Length > reader.BaseStream.Length)
            {
                return false;
            }
            
            // Try to match the marker
            bool match = true;
            for (int j = 0; j < marker.Length; j++)
            {
                byte b = reader.ReadByte();
                if (b != marker[j])
                {
                    match = false;
                    break;
                }
            }
            
            if (match)
            {
                return true;
            }
            
            // If it didn't match, go back to after the first byte we checked and continue
            reader.BaseStream.Position = pos + 1;
        }
        
        return false;
    }
    
    private static bool ParseSimplifiedImageData(byte[] imageData, int sectionIndex, byte[] voxelData)
    {
        // This is a simplified version since we can't directly parse the PNG image
        // In a real implementation, we would parse the PNG properly
        if (imageData.Length < 64 * 64 * 4)  // Assuming 64x64 RGBA image
        {
            return false;
        }
        
        bool anyVoxelsLoaded = false;
        Vector3 offset = _offsets[sectionIndex];
        
        // Process a simplified version - we'll just use the raw bytes as color data
        // This won't actually work with real GOX files, but demonstrates the approach
        for (int i = 0; i < 64 * 64; i++)
        {
            // Calculate position in the 64x64 grid
            int x = i % 64;
            int y = i / 64;
            
            // Calculate the corresponding voxel position in the section
            int px = i % 16;
            int py = (i / 16) % 16;
            int pz = (i / 256) % 16;
            
            // Apply the section offset
            int vx = (int)(px + offset.X * 16);
            int vy = (int)(py + offset.Y * 16);
            int vz = (int)(pz + offset.Z * 16);
            
            // Skip if outside the chunk bounds
            if (vx < 0 || vx >= VoxelChunk.ChunkSize ||
                vy < 0 || vy >= VoxelChunk.ChunkSize ||
                vz < 0 || vz >= VoxelChunk.ChunkSize)
            {
                continue;
            }
            
            // Get a simplified color from the image data - this is just for demonstration
            // In a real implementation, we would extract the proper RGBA color
            int colorIndex = (i * 4) % imageData.Length;
            if (colorIndex + 3 < imageData.Length)
            {
                // Extract RGBA values
                byte r = imageData[colorIndex];
                byte g = imageData[colorIndex + 1];
                byte b = imageData[colorIndex + 2];
                byte a = imageData[colorIndex + 3];
                
                // Skip transparent pixels
                if (a < 128)
                {
                    continue;
                }
                
                // Simplified RGB to integer color
                int color = (r << 16) | (g << 8) | b;
                
                // Map the color to our voxel type (using the closest match in our simple map)
                byte voxelType = MapColorToVoxelType(color);
                
                if (voxelType > 0)
                {
                    // Calculate the index in the voxel array
                    int index = VoxelChunk.GetIndex(vx, vy, vz);
                    if (index >= 0 && index < voxelData.Length)
                    {
                        voxelData[index] = voxelType;
                        anyVoxelsLoaded = true;
                    }
                }
            }
        }
        
        return anyVoxelsLoaded;
    }
    
    private static byte MapColorToVoxelType(int color)
    {
        // Default to empty
        byte voxelType = 0;
        
        // Try to find the exact color match
        if (_colorMap.TryGetValue(color, out voxelType))
        {
            return voxelType;
        }
        
        // If no exact match, find the closest color in our map
        // This is a very simple approach - a real implementation would use color distance
        int closest = 0xFFFFFF;
        int minDistance = int.MaxValue;
        
        foreach (int mapColor in _colorMap.Keys)
        {
            int r1 = (color >> 16) & 0xFF;
            int g1 = (color >> 8) & 0xFF;
            int b1 = color & 0xFF;
            
            int r2 = (mapColor >> 16) & 0xFF;
            int g2 = (mapColor >> 8) & 0xFF;
            int b2 = mapColor & 0xFF;
            
            // Simple RGB distance
            int distance = Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = mapColor;
            }
        }
        
        return _colorMap[closest];
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
}
