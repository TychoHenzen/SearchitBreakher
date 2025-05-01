using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SearchitLibrary.Graphics;

public class ChunkLoader
{
    // Offsets for the 8 sections in a GOX file
    private static readonly Vector3[] _offsets =
    {
        new(0, 0, 1), // section 1
        new(1, 0, 1), // section 2 
        new(1, 0, 0), // section 4
        new(0, 0, 0), // section 3
        new(0, 1, 0), // section 5
        new(0, 1, 1), // section 6
        new(1, 1, 1), // section 7
        new(1, 1, 0)  // section 8
    };

    // Color mapping based on the colors in the Test.txt file
    private static readonly Dictionary<int, byte> _colorMap = new Dictionary<int, byte>
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
                using (BinaryReader reader = new(stream))
                {
                    // Actually parse the GOX file format
                    int index = 0;
                    while (stream.Position < stream.Length && index < 8)
                    {
                        if (SeekToMarker(reader, "BL16"))
                        {
                            // Read the PNG data size
                            uint fileSize = reader.ReadUInt32();
                            
                            // Read the PNG data
                            byte[] pngData = reader.ReadBytes((int)fileSize);
                            
                            // Load the PNG data using ImageSharp
                            using (Image<Rgba32> img = Image.Load<Rgba32>(pngData))
                            {
                                // Process the image data for this section
                                LoadImage(index, img, voxelData);
                            }
                            
                            index++;
                        }
                        else
                        {
                            // If we can't find the marker, break out of the loop
                            break;
                        }
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
    
    private static void LoadImage(int sectionIndex, Image<Rgba32> img, byte[] voxelData)
    {
        // Iterate through all pixels in the image (assumed to be 64x64)
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                ParsePixel(sectionIndex, img, voxelData, x, y);
            }
        }
    }
    
    private static void ParsePixel(int sectionIndex, Image<Rgba32> img, byte[] voxelData, int coordX, int coordY)
    {
        // Calculate the corresponding position in 3D space based on the pixel coordinates
        int posIndex = coordX + 64 * coordY;
        int px = posIndex % 16;
        posIndex /= 16;
        int pz = posIndex % 16;
        posIndex /= 16;
        int py = posIndex;

        // Apply the section offset
        Vector3 offset = _offsets[sectionIndex];
        int vx = (int)(px + offset.X * 16);
        int vy = (int)(py + offset.Y * 16);
        int vz = (int)(pz + offset.Z * 16);
        
        // Get the pixel color
        Rgba32 pixel = img[coordX, coordY];
        
        // Convert to ARGB format
        int color = (pixel.A << 24) | (pixel.R << 16) | (pixel.G << 8) | pixel.B;
        
        // Skip transparent pixels
        if ((color & 0xFF000000) == 0)
        {
            return;
        }
        
        // Get only RGB components
        int rgbColor = color & 0x00FFFFFF;
        
        // Map color to voxel type
        byte voxelType = MapColorToVoxelType(rgbColor);
        
        // If the voxel type is 0 (empty/white), skip
        if (voxelType == 0)
        {
            return;
        }
        
        // Calculate the index in the voxel array
        int voxelIndex = VoxelChunk.GetIndex(vx, vy, vz);
        
        // Set the voxel data if the index is valid
        if (voxelIndex >= 0 && voxelIndex < voxelData.Length)
        {
            voxelData[voxelIndex] = voxelType;
        }
    }
    
    public static byte MapColorToVoxelType(int color)
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