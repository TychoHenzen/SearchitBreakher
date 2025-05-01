using System;
using System.Numerics;

namespace SearchitLibrary.Graphics;

public class VoxelChunk
{
    private readonly byte[] _voxelData;
    public Vector3 Position { get; }
    
    
    public VoxelChunk(Vector3 position)
    {
        Position = position;
        _voxelData = new byte[Constants.VoxelCount];
    }
    
    public VoxelChunk(Vector3 position, byte[] voxelData)
    {
        if (voxelData.Length != Constants.VoxelCount)
        {
            throw new ArgumentException($"Voxel data must contain exactly {Constants.VoxelCount} elements", nameof(voxelData));
        }
        
        Position = position;
        _voxelData = voxelData;
    }
    
    // Gets the voxel type at the specified position
    public byte GetVoxel(int x, int y, int z)
    {
        if (x < 0 || x >= Constants.ChunkSize || y < 0 || y >= Constants.ChunkSize || z < 0 || z >= Constants.ChunkSize)
        {
            return 0; // Return empty voxel for out-of-bounds
        }
        
        return _voxelData[Helpers.GetIndex(x, y, z)];
    }
    
    // Sets the voxel type at the specified position
    public void SetVoxel(int x, int y, int z, byte value)
    {
        if (x < 0 || x >= Constants.ChunkSize || y < 0 || y >= Constants.ChunkSize || z < 0 || z >= Constants.ChunkSize)
        {
            return; // Ignore out-of-bounds
        }
        
        _voxelData[Helpers.GetIndex(x, y, z)] = value;
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
    
    
    
    // Gets all voxel data as an array
    public byte[] GetVoxelData()
    {
        // Return a copy to prevent external modification
        byte[] copy = new byte[_voxelData.Length];
        Array.Copy(_voxelData, copy, _voxelData.Length);
        return copy;
    }
    
}