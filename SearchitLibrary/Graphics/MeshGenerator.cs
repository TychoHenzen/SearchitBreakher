using System;
using System.Collections.Generic;
using System.Numerics;

namespace SearchitLibrary.Graphics
{
    /// <summary>
    /// Generates vertex and index data for rendering voxel chunks.
    /// </summary>
    public class MeshGenerator
    {
        // Constants for vertices/indices per face
        private const int VerticesPerFace = 4;
        private const int IndicesPerFace = 6; // 2 triangles * 3 indices
        
        /// <summary>
        /// Generates mesh data for a voxel chunk.
        /// </summary>
        /// <param name="chunk">The voxel chunk</param>
        /// <param name="cameraPosition">The position of the camera for shading</param>
        /// <returns>Generated mesh data</returns>
        public static MeshData GenerateMeshForChunk(VoxelChunk chunk, Vector3 cameraPosition)
        {
            // If chunk is null, return empty mesh
            if (chunk == null)
            {
                return new MeshData(
                    Array.Empty<Vector3>(),
                    Array.Empty<Vector3>(),
                    Array.Empty<int>()
                );
            }
            
            // Calculate visible faces
            int maxFaces = VoxelFaceCalculator.CalculateVisibleFaces(chunk);
            
            // Prepare arrays for mesh data
            Vector3[] positions = new Vector3[maxFaces * VerticesPerFace];
            Vector3[] colors = new Vector3[maxFaces * VerticesPerFace];
            int[] indices = new int[maxFaces * IndicesPerFace];
            
            int vertexOffset = 0;
            int indexOffset = 0;
            
            // Iterate through all voxels in the chunk
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
                        
                        // Get the face colors for this voxel type
                        Vector3[] faceColors = VoxelColorMap.GetFaceColors(voxelType);
                        
                        // Calculate the voxel position in world space
                        Vector3 voxelPosition = new(
                            chunk.Position.X + x,
                            chunk.Position.Y + y,
                            chunk.Position.Z + z
                        );
                        
                        // Add visible faces
                        AddVisibleFaces(
                            chunk,
                            voxelPosition,
                            voxelType,
                            faceColors,
                            x, y, z,
                            positions,
                            colors,
                            indices,
                            ref vertexOffset,
                            ref indexOffset
                        );
                    }
                }
            }
            
            // Resize arrays to actual used size
            if (vertexOffset < positions.Length)
            {
                Array.Resize(ref positions, vertexOffset);
                Array.Resize(ref colors, vertexOffset);
                Array.Resize(ref indices, indexOffset);
            }
            
            // Apply shading based on camera position
            VoxelShader shader = new();
            Vector3[] shadedColors = shader.ApplyDistanceShadingBatch(positions, colors, cameraPosition);
            
            return new MeshData(positions, shadedColors, indices);
        }
        
        private static void AddVisibleFaces(
            VoxelChunk chunk,
            Vector3 position,
            byte voxelType,
            Vector3[] faceColors,
            int x, int y, int z,
            Vector3[] positions,
            Vector3[] colors,
            int[] indices,
            ref int vertexOffset,
            ref int indexOffset)
        {
            // Check each face direction
            foreach (VoxelFaceCalculator.FaceDirection direction in Enum.GetValues(typeof(VoxelFaceCalculator.FaceDirection)))
            {
                // Check if this face is visible
                if (VoxelFaceCalculator.IsFaceVisible(chunk, x, y, z, direction))
                {
                    // Get vertices for this face
                    Vector3[] faceVertices = VoxelFaceCalculator.GetFaceVertices(position, direction);
                    
                    // Get color for this face
                    Vector3 faceColor = faceColors[(int)direction];
                    
                    // Add vertices
                    for (int i = 0; i < VerticesPerFace; i++)
                    {
                        positions[vertexOffset + i] = faceVertices[i];
                        colors[vertexOffset + i] = faceColor;
                    }
                    
                    // Add indices
                    int[] faceIndices = VoxelFaceCalculator.GetFaceIndices(vertexOffset);
                    for (int i = 0; i < IndicesPerFace; i++)
                    {
                        indices[indexOffset + i] = faceIndices[i];
                    }
                    
                    vertexOffset += VerticesPerFace;
                    indexOffset += IndicesPerFace;
                }
            }
        }
        
        public class MeshData
        {
            public Vector3[] Positions { get; }
            public Vector3[] Colors { get; }
            public int[] Indices { get; }
            
            public MeshData(Vector3[] positions, Vector3[] colors, int[] indices)
            {
                Positions = positions;
                Colors = colors;
                Indices = indices;
            }
        }
    }
}