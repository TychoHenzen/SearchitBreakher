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
        public static MeshData GenerateMeshForChunk(VoxelChunk? chunk, Vector3 cameraPosition)
        {
            // If chunk is null, return empty mesh
            if (chunk == null)
            {
                return new MeshData([], [], []);
            }

            // Calculate visible faces
            var maxFaces = VoxelVisibility.CalculateVisibleFaces(chunk);

            // Prepare arrays for mesh data
            var positions = new Vector3[maxFaces * VerticesPerFace];
            var colors = new Vector3[maxFaces * VerticesPerFace];
            var indices = new int[maxFaces * IndicesPerFace];

            int vertexOffset = 0;
            int indexOffset = 0;

            // Iterate through all voxels in the chunk
            var positions1 = positions;
            var colors1 = colors;
            var indices1 = indices;
            Helpers.Foreach3(Constants.ChunkSize, position =>
            {
                var voxelType = chunk.GetVoxel(position);

                // Skip empty voxels
                if (voxelType == 0) return;

                // Get the face colors for this voxel type
                var faceColors = VoxelColorMap.GetFaceColors(voxelType);

                // Calculate the voxel position in world space
                var voxelPosition = chunk.Position + position;

                // Add visible faces
                AddVisibleFaces(
                    chunk,
                    voxelPosition,
                    voxelType,
                    faceColors,
                    position,
                    positions1,
                    colors1,
                    indices1,
                    ref vertexOffset,
                    ref indexOffset
                );
            });


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
            Vector3 chunkPosition,
            Vector3[] positions,
            Vector3[] colors,
            int[] indices,
            ref int vertexOffset,
            ref int indexOffset)
        {
            // Check each face direction
            foreach (VoxelVisibility.FaceDirection direction in Enum.GetValues(
                         typeof(VoxelVisibility.FaceDirection)))
            {
                // Check if this face is visible
                if (!VoxelVisibility.IsFaceVisible(chunk, chunkPosition, direction)) continue;
                // Get vertices for this face
                var faceVertices = VoxelVisibility.GetFaceVertices(position, direction);

                // Get color for this face
                var faceColor = faceColors[(int)direction];

                // Add vertices
                for (var i = 0; i < VerticesPerFace; i++)
                {
                    positions[vertexOffset + i] = faceVertices[i];
                    colors[vertexOffset + i] = faceColor;
                }

                // Add indices
                var faceIndices = VoxelVisibility.GetFaceIndices(vertexOffset);
                for (var i = 0; i < IndicesPerFace; i++) indices[indexOffset + i] = faceIndices[i];

                vertexOffset += VerticesPerFace;
                indexOffset += IndicesPerFace;
            }
        }

        public class MeshData
        {
            public MeshData(Vector3[] positions, Vector3[] colors, int[] indices)
            {
                Positions = positions;
                Colors = colors;
                Indices = indices;
            }

            public Vector3[] Positions { get; }
            public Vector3[] Colors { get; }
            public int[] Indices { get; }
        }
    }
}