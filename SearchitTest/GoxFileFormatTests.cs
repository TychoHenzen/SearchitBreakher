using NUnit.Framework;
using SearchitLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SearchitTest;

[TestFixture]
public class GoxFileFormatTests
{
    private const string TestTextPath = "../../../TestVoxels/Test.txt";
    private const string TestGoxPath = "../../../TestVoxels/Test.gox";
    
    // Color mapping based on the colors in the Test.txt file
    private static readonly Dictionary<int, byte> ColorMapping = new()
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
    
    [Test]
    public void LoadingTextAndGoxFiles_ProducesSameVoxelData()
    {
        // Arrange
        string textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, TestTextPath);
        string goxFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, TestGoxPath);
        
        // Ensure the test files exist
        Assert.That(File.Exists(textFilePath), Is.True, $"Test text file not found at {textFilePath}");
        Assert.That(File.Exists(goxFilePath), Is.True, $"Test GOX file not found at {goxFilePath}");
        
        // Act
        byte[] textVoxels = LoadVoxelTextFile(textFilePath);
        VoxelChunk goxChunk = ChunkLoader.LoadGoxFile(goxFilePath, Vector3.Zero);
        byte[] goxVoxels = goxChunk.GetVoxelData();

        // Output non-zero voxels from GOX file
        for (int i = 0; i < goxVoxels.Length; i++)
        {
            if (goxVoxels[i] > 0)
            {
                Vector3 pos = VoxelChunk.GetPosition(i);
                Console.WriteLine($"Loaded from GOX: [{pos.X}, {pos.Y}, {pos.Z}]: {goxVoxels[i]}");
            }
        }
        
        // Count non-zero voxels to ensure we're loading something
        int textNonZeroCount = textVoxels.Count(v => v > 0);
        int goxNonZeroCount = goxVoxels.Count(v => v > 0);
        
        Console.WriteLine($"Text file non-zero voxels: {textNonZeroCount}");
        Console.WriteLine($"GOX file non-zero voxels: {goxNonZeroCount}");
        
        // Assert
        bool areEqual = textVoxels.SequenceEqual(goxVoxels);
        
        if (!areEqual)
        {
            StringBuilder diffBuilder = new();
            int minLength = Math.Min(textVoxels.Length, goxVoxels.Length);
            
            int differenceCount = 0;
            for (int i = 0; i < minLength; i++)
            {
                if (textVoxels[i] != goxVoxels[i] && (textVoxels[i] > 0 || goxVoxels[i] > 0))
                {
                    // Only log differences where either file has a non-zero value
                    Vector3 pos = VoxelChunk.GetPosition(i);
                    diffBuilder.AppendLine(
                        $"Difference at index {i} [x={pos.X}, y={pos.Y}, z={pos.Z}]: TextVoxels = {textVoxels[i]}, GoxVoxels = {goxVoxels[i]}");
                    differenceCount++;
                    
                    // Limit output to first 20 differences
                    if (differenceCount >= 20)
                    {
                        diffBuilder.AppendLine($"... and {minLength - i} more potentially different indices");
                        break;
                    }
                }
            }
            
            if (textVoxels.Length != goxVoxels.Length)
            {
                diffBuilder.AppendLine(
                    $"Length mismatch: TextVoxels = {textVoxels.Length}, GoxVoxels = {goxVoxels.Length}");
            }
            
            Console.WriteLine($"Found {differenceCount} differences:");
            Console.WriteLine(diffBuilder.ToString());
        }
        
        Assert.That(areEqual, Is.True, "Text file and GOX file should produce the same voxel data");
    }
    
    /// <summary>
    /// Loads voxel data from a text file, similar to the reference implementation
    /// </summary>
    private byte[] LoadVoxelTextFile(string filePath)
    {
        // Initialize voxel data array (32x32x32)
        byte[] voxels = new byte[VoxelChunk.ChunkSize * VoxelChunk.ChunkSize * VoxelChunk.ChunkSize];
        
        try
        {
            // Read the text file
            string[] lines = File.ReadAllLines(filePath);
            
            foreach (string line in lines)
            {
                // Skip comments and empty lines
                if (line.Contains('#') || string.IsNullOrWhiteSpace(line))
                    continue;
                
                // Split the line into values
                string[] values = line.Split(' ');
                if (values.Length < 4)
                    continue;
                
                // Parse coordinates and color
                int x = int.Parse(values[0]) + 16;
                int z = int.Parse(values[1]) + 16;
                int y = int.Parse(values[2]);
                int color = Convert.ToInt32(values[3], 16);
                
                
                byte voxelType = MapColorToVoxelType(color);
                Console.WriteLine($"Loaded from text: [{x}, {y}, {z}]: {voxelType}");
                
                // Calculate index using our formula
                int index = VoxelChunk.GetIndex(x, y, z);
                
                // Map color to voxel type
                
                // Set the voxel value
                if (index >= 0 && index < voxels.Length)
                {
                    voxels[index] = voxelType;
                }
                else
                {
                    Console.WriteLine($"Warning: Index {index} out of bounds for coords [{x}, {y}, {z}]");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading text file: {ex.Message}");
            Assert.Fail($"Error loading text file: {ex.Message}");
        }
        
        return voxels;
    }
    
    /// <summary>
    /// Maps a color code to a voxel type using the ColorMapping dictionary
    /// </summary>
    private byte MapColorToVoxelType(int color)
    {
        // Use our color mapping dictionary
        color = color & 0x00FFFFFF; // Remove any alpha channel
        
        if (ColorMapping.TryGetValue(color, out byte voxelType))
        {
            return voxelType;
        }
        
        // If no exact match, find the closest color
        int closest = 0xFFFFFF;
        int minDistance = int.MaxValue;
        
        foreach (int mapColor in ColorMapping.Keys)
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
        
        Console.WriteLine($"No exact match for color {color:X}, using closest: {closest:X}");
        return ColorMapping[closest];
    }
}