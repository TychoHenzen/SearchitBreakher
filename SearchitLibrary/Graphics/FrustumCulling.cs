using System.Numerics;

namespace SearchitLibrary.Graphics
{
    /// <summary>
    /// Provides frustum culling functionality for voxel chunks.
    /// Handles both perspective and orthographic projections uniformly via plane extraction.
    /// </summary>
    public static class FrustumCulling
    {
        /// <summary>
        /// Tests if a chunk intersects with the view frustum.
        /// </summary>
        /// <param name="chunkPosition">The center position of the chunk</param>
        /// <param name="chunkSize">The size (edge length) of the chunk</param>
        /// <param name="viewProjectionMatrix">The combined view-projection matrix</param>
        /// <returns>True if the chunk is visible</returns>
        public static bool IsChunkVisible(Vector3 chunkPosition, float chunkSize, Matrix4x4 viewProjectionMatrix)
        {
            var halfSize = chunkSize * 0.5f;
            var min = chunkPosition - new Vector3(halfSize);
            var max = chunkPosition + new Vector3(halfSize);

            // Extract all six frustum planes (left, right, bottom, top, near, far)
            var planes = ExtractFrustumPlanes(viewProjectionMatrix);
            foreach (var plane in planes)
            {
                if (IsBoxOutsidePlane(min, max, plane))
                    return false;
            }

            return true;
        }

        private static Plane[] ExtractFrustumPlanes(Matrix4x4 m)
        {
            Plane[] planes = new Plane[6];

            // Left plane (row4 + row1)
            planes[0] = new Plane(
                m.M41 + m.M11,
                m.M42 + m.M12,
                m.M43 + m.M13,
                m.M44 + m.M14
            );
            // Right plane (row4 - row1)
            planes[1] = new Plane(
                m.M41 - m.M11,
                m.M42 - m.M12,
                m.M43 - m.M13,
                m.M44 - m.M14
            );
            // Bottom plane (row4 + row2)
            planes[2] = new Plane(
                m.M41 + m.M21,
                m.M42 + m.M22,
                m.M43 + m.M23,
                m.M44 + m.M24
            );
            // Top plane (row4 - row2)
            planes[3] = new Plane(
                m.M41 - m.M21,
                m.M42 - m.M22,
                m.M43 - m.M23,
                m.M44 - m.M24
            );
            // Near plane (row4 + row3)
            planes[4] = new Plane(
                m.M41 + m.M31,
                m.M42 + m.M32,
                m.M43 + m.M33,
                m.M44 + m.M34
            );
            // Far plane (row4 - row3)
            planes[5] = new Plane(
                m.M41 - m.M31,
                m.M42 - m.M32,
                m.M43 - m.M33,
                m.M44 - m.M34
            );

            const float eps = 1e-6f;
            for (var i = 0; i < planes.Length; i++)
            {
                var p = planes[i];
                var n = p.Normal;
                var length = MathF.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);
                if (length < eps) continue;
                planes[i] = new Plane(n / length, p.D / length);
            }

            return planes;
        }

        private static bool IsBoxOutsidePlane(Vector3 min, Vector3 max, Plane plane)
        {
            // Pick the corner of the AABB that is most likely to be outside
            Vector3 p = new Vector3(
                plane.Normal.X >= 0 ? max.X : min.X,
                plane.Normal.Y >= 0 ? max.Y : min.Y,
                plane.Normal.Z >= 0 ? max.Z : min.Z
            );

            return Vector3.Dot(plane.Normal, p) + plane.D < 0;
        }
    }
}