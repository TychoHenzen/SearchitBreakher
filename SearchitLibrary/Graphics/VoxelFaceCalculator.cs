using System.Numerics;

namespace SearchitLibrary.Graphics
{
    /// <summary>
    /// Calculates which faces of voxels should be visible based on neighbor voxels.
    /// </summary>
    public class VoxelVisibility
    {
        public enum FaceDirection
        {
            Front, // Negative Z
            Back, // Positive Z
            Left, // Negative X
            Right, // Positive X
            Top, // Positive Y
            Bottom // Negative Y
        }

        // Precompute the offset vector for each FaceDirection
        private static readonly Dictionary<FaceDirection, Vector3> _offsets = new()
        {
            [FaceDirection.Front] = -Vector3.UnitZ,
            [FaceDirection.Back] = Vector3.UnitZ,
            [FaceDirection.Left] = -Vector3.UnitX,
            [FaceDirection.Right] = Vector3.UnitX,
            [FaceDirection.Bottom] = -Vector3.UnitY,
            [FaceDirection.Top] = Vector3.UnitY
        };

        /// <summary>
        ///     Returns true if the face in the given direction has no voxel neighbor (or is at the chunk boundary).
        /// </summary>
        public static bool IsFaceVisible(VoxelChunk chunk, Vector3 pos, FaceDirection dir)
        {
            var neighborPos = pos + _offsets[dir];

            // out of bounds => face is exposed
            if (neighborPos.X < 0 || neighborPos.Y < 0 || neighborPos.Z < 0 ||
                neighborPos.X >= Constants.ChunkSize ||
                neighborPos.Y >= Constants.ChunkSize ||
                neighborPos.Z >= Constants.ChunkSize)
                return true;

            // in‐bounds => check if that neighbor is empty
            return chunk.GetVoxel(neighborPos) == 0;
        }

        /// <summary>
        /// Counts all exposed faces across every non‐empty voxel in the chunk.
        /// </summary>
        public static int CalculateVisibleFaces(VoxelChunk chunk)
        {
            int visibleFaces = 0;

            Helpers.Foreach3(Constants.ChunkSize, pos =>
            {
                if (chunk.GetVoxel(pos) == 0)
                    return;

                // for each direction, increment if that face is exposed
                foreach (var dir in _offsets.Keys)
                    if (IsFaceVisible(chunk, pos, dir))
                        visibleFaces++;
            });

            return visibleFaces;
        }


        /// <summary>
        /// Gets the vertices for a specific face of a voxel.
        /// </summary>
        public static Vector3[] GetFaceVertices(Vector3 position, FaceDirection direction, float size = 1.0f)
        {
            float halfSize = size / 2.0f;

            return direction switch
            {
                FaceDirection.Front =>
                [
                    position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
                    position + new Vector3(halfSize, -halfSize, -halfSize), // front bottom right
                    position + new Vector3(-halfSize, halfSize, -halfSize), // front top left
                    position + new Vector3(halfSize, halfSize, -halfSize) // front top right
                ],
                FaceDirection.Back =>
                [
                    position + new Vector3(halfSize, -halfSize, halfSize), // back bottom right
                    position + new Vector3(-halfSize, -halfSize, halfSize), // back bottom left
                    position + new Vector3(halfSize, halfSize, halfSize), // back top right
                    position + new Vector3(-halfSize, halfSize, halfSize) // back top left
                ],
                FaceDirection.Left =>
                [
                    position + new Vector3(-halfSize, -halfSize, halfSize), // back bottom left
                    position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
                    position + new Vector3(-halfSize, halfSize, halfSize), // back top left
                    position + new Vector3(-halfSize, halfSize, -halfSize) // front top left
                ],
                FaceDirection.Right =>
                [
                    position + new Vector3(halfSize, -halfSize, -halfSize), // front bottom right
                    position + new Vector3(halfSize, -halfSize, halfSize), // back bottom right
                    position + new Vector3(halfSize, halfSize, -halfSize), // front top right
                    position + new Vector3(halfSize, halfSize, halfSize) // back top right
                ],
                FaceDirection.Top =>
                [
                    position + new Vector3(-halfSize, halfSize, -halfSize), // front top left
                    position + new Vector3(halfSize, halfSize, -halfSize), // front top right
                    position + new Vector3(-halfSize, halfSize, halfSize), // back top left
                    position + new Vector3(halfSize, halfSize, halfSize) // back top right
                ],
                FaceDirection.Bottom =>
                [
                    position + new Vector3(-halfSize, -halfSize, -halfSize), // front bottom left
                    position + new Vector3(halfSize, -halfSize, -halfSize), // front bottom right
                    position + new Vector3(-halfSize, -halfSize, halfSize), // back bottom left
                    position + new Vector3(halfSize, -halfSize, halfSize) // back bottom right
                ],
                _ => []
            };
        }

        /// <summary>
        /// Gets indices for a face (assumes vertices are ordered consistently).
        /// </summary>
        public static int[] GetFaceIndices(int baseIndex)
        {
            return
            [
                baseIndex, baseIndex + 1, baseIndex + 2, // Triangle 1
                baseIndex + 2, baseIndex + 1, baseIndex + 3 // Triangle 2
            ];
        }
    }
}