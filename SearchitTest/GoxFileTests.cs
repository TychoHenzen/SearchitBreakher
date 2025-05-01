using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using SearchitLibrary.Graphics;

namespace SearchitLibrary.Tests;

/// <summary>
/// Test class for validating GOX file loading.
/// Similar to the reference implementation's GoxFileFormatTests.cs
/// </summary>
public static class GoxFileTests
{
    /// <summary>
    /// Tests that a GOX file and its corresponding text file produce the same voxel data
    /// </summary>
    public static bool TestGoxLoading(string textFilePath, string goxFilePath)
    {
        try
        {
            // Load the text file voxels
            byte[] textVoxels = LoadVoxelTextFile(textFilePath);
            
            // Load the GOX file voxels
            VoxelChunk goxChunk = ChunkLoader.LoadGoxFile(goxFilePath, Vector3.Zero);
            byte[] goxVoxels = goxChunk.GetVoxelData();
            
            // Compare the results
            if (textVoxels.SequenceEqual(goxVoxels))
            {
                Console.WriteLine("Test passed: Text and GOX files produce identical voxel data");
                return true;
            }
            
            // Log differences for debugging
            StringBuilder diffBuilder = new StringBuilder();
            int minLength = Math.Min(textVoxels.Length, goxVoxels.Length);
            
            int differenceCount = 0;
            for (int i = 0; i < minLength; i++)
            {
                if (textVoxels[i] != goxVoxels[i])
                {
                    // Only log the first 100 differences to avoid excessive output
                    if (differenceCount < 100)
                    {
                        Vector3 pos = VoxelChunk.GetPosition(i);
                        diffBuilder.AppendLine(
                            $"Difference at index {i} [x={pos.X}, y={pos.Y}, z={pos.Z}]: TextVoxels = {textVoxels[i]}, GoxVoxels = {goxVoxels[i]}");
                    }
                    differenceCount++;
                }
            }
            
            if (textVoxels.Length != goxVoxels.Length)
            {
                diffBuilder.AppendLine(
                    $"Length mismatch: TextVoxels = {textVoxels.Length}, GoxVoxels = {goxVoxels.Length}");
            }
            
            Console.WriteLine($"Test failed: Found {differenceCount} differences between text and GOX voxel data");
            Console.WriteLine(diffBuilder.ToString());
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during test: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads voxel data from a text file, similar to the reference implementation
    /// </summary>
    private static byte[] LoadVoxelTextFile(string filePath)
    {
        // Initialize voxel data array (32x32x32)
        byte[] voxels = new byte[VoxelChunk.ChunkSize * VoxelChunk.ChunkSize * VoxelChunk.ChunkSize];
        
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
            int y = int.Parse(values[1]) + 16;
            int z = int.Parse(values[2]);
            int color = Convert.ToInt32(values[3], 16);
            
            
            // Calculate index using our updated formula
            int index = VoxelChunk.GetIndex(x, y, z);
            
            byte voxelType = color.MapColorToVoxelType();
            
            // Set the voxel value
            if (index >= 0 && index < voxels.Length)
            {
                voxels[index] = voxelType;
            }
        }
        
        return voxels;
    }
    
}
