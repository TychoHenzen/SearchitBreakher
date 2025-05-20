using System.Numerics;

namespace SearchitLibrary.Graphics
{
    /// <summary>
    /// Defines standard colors for different voxel types.
    /// </summary>
    public class VoxelColorMap
    {
        private static readonly Dictionary<byte, Vector3[]> DefaultColorMap;

        static VoxelColorMap()
        {
            // Initialize the default color map 
            DefaultColorMap = new Dictionary<byte, Vector3[]>
            {
                // Default/empty voxel (shouldn't be rendered)
                {
                    0, [
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, 0.0f)
                    ]
                },

                // Type 1: Red-themed voxel
                {
                    1, [
                        new Vector3(1.0f, 0.2f, 0.2f), // Front
                        new Vector3(0.9f, 0.1f, 0.1f), // Back
                        new Vector3(0.8f, 0.0f, 0.0f), // Left
                        new Vector3(0.7f, 0.0f, 0.0f), // Right
                        new Vector3(0.6f, 0.0f, 0.0f), // Top
                        new Vector3(0.5f, 0.0f, 0.0f) // Bottom
                    ]
                },

                // Type 2: Green-themed voxel
                {
                    2, [
                        new Vector3(0.2f, 1.0f, 0.2f), // Front
                        new Vector3(0.1f, 0.9f, 0.1f), // Back
                        new Vector3(0.0f, 0.8f, 0.0f), // Left
                        new Vector3(0.0f, 0.7f, 0.0f), // Right
                        new Vector3(0.0f, 0.6f, 0.0f), // Top
                        new Vector3(0.0f, 0.5f, 0.0f) // Bottom
                    ]
                },

                // Type 3: Blue-themed voxel
                {
                    3, [
                        new Vector3(0.2f, 0.2f, 1.0f), // Front
                        new Vector3(0.1f, 0.1f, 0.9f), // Back
                        new Vector3(0.0f, 0.0f, 0.8f), // Left
                        new Vector3(0.0f, 0.0f, 0.7f), // Right
                        new Vector3(0.0f, 0.0f, 0.6f), // Top
                        new Vector3(0.0f, 0.0f, 0.5f) // Bottom
                    ]
                },

                // Type 4: Yellow-themed voxel
                {
                    4, [
                        new Vector3(1.0f, 1.0f, 0.2f), // Front
                        new Vector3(0.9f, 0.9f, 0.1f), // Back
                        new Vector3(0.8f, 0.8f, 0.0f), // Left
                        new Vector3(0.7f, 0.7f, 0.0f), // Right
                        new Vector3(0.6f, 0.6f, 0.0f), // Top
                        new Vector3(0.5f, 0.5f, 0.0f) // Bottom
                    ]
                },

                // Type 5: Cyan-themed voxel
                {
                    5, [
                        new Vector3(0.2f, 1.0f, 1.0f), // Front
                        new Vector3(0.1f, 0.9f, 0.9f), // Back
                        new Vector3(0.0f, 0.8f, 0.8f), // Left
                        new Vector3(0.0f, 0.7f, 0.7f), // Right
                        new Vector3(0.0f, 0.6f, 0.6f), // Top
                        new Vector3(0.0f, 0.5f, 0.5f) // Bottom
                    ]
                },

                // Type 6: Magenta-themed voxel
                {
                    6, [
                        new Vector3(1.0f, 0.2f, 1.0f), // Front
                        new Vector3(0.9f, 0.1f, 0.9f), // Back
                        new Vector3(0.8f, 0.0f, 0.8f), // Left
                        new Vector3(0.7f, 0.0f, 0.7f), // Right
                        new Vector3(0.6f, 0.0f, 0.6f), // Top
                        new Vector3(0.5f, 0.0f, 0.5f) // Bottom
                    ]
                }
            };
        }

        /// <summary>
        /// Gets the face colors for a specific voxel type.
        /// </summary>
        /// <param name="voxelType">The type of voxel</param>
        /// <returns>An array of colors for each face</returns>
        public static Vector3[] GetFaceColors(byte voxelType)
        {
            if (DefaultColorMap.TryGetValue(voxelType, out var colors))
            {
                return colors;
            }

            // Default to type 1 if not found
            return DefaultColorMap[1];
        }

        /// <summary>
        /// Gets the color for a specific face of a voxel.
        /// </summary>
        public static Vector3 GetFaceColor(byte voxelType, VoxelVisibility.FaceDirection direction)
        {
            var colors = GetFaceColors(voxelType);
            return colors[(int)direction];
        }
    }
}