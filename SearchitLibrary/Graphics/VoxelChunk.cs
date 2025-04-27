using System;
using System.Numerics;

namespace SearchitLibrary.Graphics;

public class VoxelChunk
{
    public const int ChunkSize = 32;
    private readonly byte[] _voxelData;
    public Vector3 Position { get; }
    
    // Total number of voxels in the chunk
    public int VoxelCount => ChunkSize * ChunkSize * ChunkSize;
    
    public VoxelChunk(Vector3 position)
    {
        Position = position;
        _voxelData = new byte[VoxelCount];
    }
    
    public VoxelChunk(Vector3 position, byte[] voxelData)
    {
        if (voxelData.Length != VoxelCount)
        {
            throw new ArgumentException($"Voxel data must contain exactly {VoxelCount} elements", nameof(voxelData));
        }
        
        Position = position;
        _voxelData = voxelData;
    }
    
    // Gets the voxel type at the specified position
    public byte GetVoxel(int x, int y, int z)
    {
        if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize || z < 0 || z >= ChunkSize)
        {
            return 0; // Return empty voxel for out-of-bounds
        }
        
        return _voxelData[GetIndex(x, y, z)];
    }
    
    // Sets the voxel type at the specified position
    public void SetVoxel(int x, int y, int z, byte value)
    {
        if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize || z < 0 || z >= ChunkSize)
        {
            return; // Ignore out-of-bounds
        }
        
        _voxelData[GetIndex(x, y, z)] = value;
    }
    
    // Gets a voxel at the specified local position
    public byte GetVoxel(Vector3 localPosition)
    {
        int x = (int)MathF.Floor(localPosition.X);
        int y = (int)MathF.Floor(localPosition.Y);
        int z = (int)MathF.Floor(localPosition.Z);
        
        return GetVoxel(x, y, z);
    }
    
    // Sets a voxel at the specified local position
    public void SetVoxel(Vector3 localPosition, byte value)
    {
        int x = (int)MathF.Floor(localPosition.X);
        int y = (int)MathF.Floor(localPosition.Y);
        int z = (int)MathF.Floor(localPosition.Z);
        
        SetVoxel(x, y, z, value);
    }
    
    // Converts coordinates to an index in the data array
    public static int GetIndex(int x, int y, int z)
    {
        // Ensure coordinates are within bounds
        x = Math.Clamp(x, 0, ChunkSize - 1);
        y = Math.Clamp(y, 0, ChunkSize - 1);
        z = Math.Clamp(z, 0, ChunkSize - 1);
        
        // Fix: Proper flattening of 3D coordinates to 1D array
        // Using z-major ordering (x changes most slowly, then y, then z)
        return x + (y * ChunkSize) + (z * ChunkSize * ChunkSize);
    }
    
    // Converts an index back to coordinates
    public static Vector3 GetPosition(int index)
    {
        // Fix: Match the above GetIndex formula by reversing it
        int x = index % ChunkSize;
        index /= ChunkSize;
        int y = index % ChunkSize;
        index /= ChunkSize;
        int z = index;
        
        return new Vector3(x, y, z);
    }
    
    // Gets all voxel data as an array
    public byte[] GetVoxelData()
    {
        // Return a copy to prevent external modification
        byte[] copy = new byte[_voxelData.Length];
        Array.Copy(_voxelData, copy, _voxelData.Length);
        return copy;
    }
    
    // Creates a test chunk with a pattern for debugging
    public static VoxelChunk CreateTestChunk(Vector3 position)
    {
        VoxelChunk chunk = new(position);
        
        // Create a simple pattern - a hollow box in the center
        int centerStart = ChunkSize / 4;
        int centerEnd = ChunkSize - centerStart;
        
        for (int x = centerStart; x < centerEnd; x++)
        {
            for (int y = centerStart; y < centerEnd; y++)
            {
                for (int z = centerStart; z < centerEnd; z++)
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
}
