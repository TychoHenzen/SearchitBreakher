using System;
using System.Numerics;

namespace SearchitLibrary.Graphics
{
    /// <summary>
    /// Calculates which faces of voxels should be visible based on neighbor voxels.
    /// </summary>
    public class VoxelFaceCalculator
    {
        /// <summary>
        /// Calculates how many faces of a chunk are visible.
        /// </summary>
        /// <param name="chunk">The voxel chunk to analyze</param>
        /// <returns>The number of visible faces</returns>
        public static int CalculateVisibleFaces(VoxelChunk chunk)
        {
            int visibleFaces = 0;
            
            for (int x = 0; x < Constants.ChunkSize; x++)
            {
                for (int y = 0; y < Constants.ChunkSize; y++)
                {
                    for (int z = 0; z < Constants.ChunkSize; z++)
                    {
                        byte voxelType = chunk.GetVoxel(x, y, z);
                        
                        // Skip empty voxels
                        if (voxelType == 0)
                        {
                            continue;
                        }
                        
                        // Check each adjacent voxel
                        // Front face (negative Z)
                        if (z == 0 || chunk.GetVoxel(x, y, z - 1) == 0)
                        {
                            visibleFaces++;
                        }
                        
                        // Back face (positive Z)
                        if (z == Constants.ChunkSize - 1 || chunk.GetVoxel(x, y, z + 1) == 0)
                        {
                            visibleFaces++;
                        }
                        
                        // Left face (negative X)
                        if (x == 0 || chunk.GetVoxel(x - 1, y, z) == 0)
                        {
                            visibleFaces++;
                        }
                        
                        // Right face (positive X)
                        if (x == Constants.ChunkSize - 1 || chunk.GetVoxel(x + 1, y, z) == 0)
                        {
                            visibleFaces++;
                        }
                        
                        // Bottom face (negative Y)
                        if (y == 0 || chunk.GetVoxel(x, y - 1, z) == 0)
                        {
                            visibleFaces++;
                        }
                        
                        // Top face (positive Y)
                        if (y == Constants.ChunkSize - 1 || chunk.GetVoxel(x, y + 1, z) == 0)
                        {
                            visibleFaces++;
                        }
                    }
                }
            }
            
            return visibleFaces;
        }
        
        /// <summary>
        /// Determines if a face should be visible based on neighboring voxels.
        /// </summary>
        public static bool IsFaceVisible(VoxelChunk chunk, int x, int y, int z, FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.Front => z == 0 || chunk.GetVoxel(x, y, z - 1) == 0,
                FaceDirection.Back => z == Constants.ChunkSize - 1 || chunk.GetVoxel(x, y, z + 1) == 0,
                FaceDirection.Left => x == 0 || chunk.GetVoxel(x - 1, y, z) == 0,
                FaceDirection.Right => x == Constants.ChunkSize - 1 || chunk.GetVoxel(x + 1, y, z) == 0,
                FaceDirection.Bottom => y == 0 || chunk.GetVoxel(x, y - 1, z) == 0,
                FaceDirection.Top => y == Constants.ChunkSize - 1 || chunk.GetVoxel(x, y + 1, z) == 0,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets the vertices for a specific face of a voxel.
        /// </summary>
        public static Vector3[] GetFaceVertices(Vector3 position, FaceDirection direction, float size = 1.0f)
        {
            float halfSize = size / 2.0f;
            
            return direction switch
            {
                FaceDirection.Front => new[] {
                    position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
                    position + new Vector3(halfSize, -halfSize, -halfSize),  // front bottom right
                    position + new Vector3(-halfSize, halfSize, -halfSize),  // front top left
                    position + new Vector3(halfSize, halfSize, -halfSize)    // front top right
                },
                FaceDirection.Back => new[] {
                    position + new Vector3(halfSize, -halfSize, halfSize),   // back bottom right
                    position + new Vector3(-halfSize, -halfSize, halfSize),  // back bottom left
                    position + new Vector3(halfSize, halfSize, halfSize),    // back top right
                    position + new Vector3(-halfSize, halfSize, halfSize)    // back top left
                },
                FaceDirection.Left => new[] {
                    position + new Vector3(-halfSize, -halfSize, halfSize),  // back bottom left
                    position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
                    position + new Vector3(-halfSize, halfSize, halfSize),   // back top left
                    position + new Vector3(-halfSize, halfSize, -halfSize)   // front top left
                },
                FaceDirection.Right => new[] {
                    position + new Vector3(halfSize, -halfSize, -halfSize),  // front bottom right
                    position + new Vector3(halfSize, -halfSize, halfSize),   // back bottom right
                    position + new Vector3(halfSize, halfSize, -halfSize),   // front top right
                    position + new Vector3(halfSize, halfSize, halfSize)     // back top right
                },
                FaceDirection.Top => new[] {
                    position + new Vector3(-halfSize, halfSize, -halfSize),  // front top left
                    position + new Vector3(halfSize, halfSize, -halfSize),   // front top right
                    position + new Vector3(-halfSize, halfSize, halfSize),   // back top left
                    position + new Vector3(halfSize, halfSize, halfSize)     // back top right
                },
                FaceDirection.Bottom => new[] {
                    position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
                    position + new Vector3(halfSize, -halfSize, -halfSize),  // front bottom right
                    position + new Vector3(-halfSize, -halfSize, halfSize),  // back bottom left
                    position + new Vector3(halfSize, -halfSize, halfSize)    // back bottom right
                },
                _ => Array.Empty<Vector3>()
            };
        }
        
        /// <summary>
        /// Gets indices for a face (assumes vertices are ordered consistently).
        /// </summary>
        public static int[] GetFaceIndices(int baseIndex)
        {
            return new int[]
            {
                baseIndex, baseIndex + 1, baseIndex + 2,    // Triangle 1
                baseIndex + 2, baseIndex + 1, baseIndex + 3 // Triangle 2
            };
        }
        
        public enum FaceDirection
        {
            Front,  // Negative Z
            Back,   // Positive Z
            Left,   // Negative X
            Right,  // Positive X
            Top,    // Positive Y
            Bottom  // Negative Y
        }
    }
}