using System;
using System.Numerics;

namespace SearchitLibrary.Graphics
{
    /// <summary>
    /// Provides frustum culling functionality for voxel chunks.
    /// </summary>
    public class FrustumCulling
    {
        /// <summary>
        /// Tests if a chunk intersects with the view frustum.
        /// </summary>
        /// <param name="chunkPosition">The position of the chunk</param>
        /// <param name="chunkSize">The size of the chunk</param>
        /// <param name="viewProjectionMatrix">The combined view-projection matrix</param>
        /// <returns>True if the chunk is visible</returns>
        public static bool IsChunkVisible(Vector3 chunkPosition, float chunkSize, Matrix4x4 viewProjectionMatrix)
        {
            // Create a bounding box for the chunk
            Vector3 min = chunkPosition;
            Vector3 max = chunkPosition + new Vector3(chunkSize, chunkSize, chunkSize);
            
            // Extract frustum planes from the view-projection matrix
            Plane[] frustumPlanes = ExtractFrustumPlanes(viewProjectionMatrix);
            
            // Test each corner of the box against each plane
            foreach (var plane in frustumPlanes)
            {
                // If all corners are outside any plane, the box is outside the frustum
                if (IsBoxOutsidePlane(min, max, plane))
                {
                    return false;
                }
            }
            
            // The box intersects or is inside the frustum
            return true;
        }
        
        private static Plane[] ExtractFrustumPlanes(Matrix4x4 viewProjectionMatrix)
        {
            // Extract the six frustum planes from the view-projection matrix
            Plane[] planes = new Plane[6];
            
            // Left plane
            planes[0] = new Plane(
                viewProjectionMatrix.M14 + viewProjectionMatrix.M11,
                viewProjectionMatrix.M24 + viewProjectionMatrix.M21,
                viewProjectionMatrix.M34 + viewProjectionMatrix.M31,
                viewProjectionMatrix.M44 + viewProjectionMatrix.M41
            );
            
            // Right plane
            planes[1] = new Plane(
                viewProjectionMatrix.M14 - viewProjectionMatrix.M11,
                viewProjectionMatrix.M24 - viewProjectionMatrix.M21,
                viewProjectionMatrix.M34 - viewProjectionMatrix.M31,
                viewProjectionMatrix.M44 - viewProjectionMatrix.M41
            );
            
            // Bottom plane
            planes[2] = new Plane(
                viewProjectionMatrix.M14 + viewProjectionMatrix.M12,
                viewProjectionMatrix.M24 + viewProjectionMatrix.M22,
                viewProjectionMatrix.M34 + viewProjectionMatrix.M32,
                viewProjectionMatrix.M44 + viewProjectionMatrix.M42
            );
            
            // Top plane
            planes[3] = new Plane(
                viewProjectionMatrix.M14 - viewProjectionMatrix.M12,
                viewProjectionMatrix.M24 - viewProjectionMatrix.M22,
                viewProjectionMatrix.M34 - viewProjectionMatrix.M32,
                viewProjectionMatrix.M44 - viewProjectionMatrix.M42
            );
            
            // Near plane
            planes[4] = new Plane(
                viewProjectionMatrix.M13,
                viewProjectionMatrix.M23,
                viewProjectionMatrix.M33,
                viewProjectionMatrix.M43
            );
            
            // Far plane
            planes[5] = new Plane(
                viewProjectionMatrix.M14 - viewProjectionMatrix.M13,
                viewProjectionMatrix.M24 - viewProjectionMatrix.M23,
                viewProjectionMatrix.M34 - viewProjectionMatrix.M33,
                viewProjectionMatrix.M44 - viewProjectionMatrix.M43
            );
            
            // Normalize the planes
            for (int i = 0; i < 6; i++)
            {
                float length = MathF.Sqrt(
                    planes[i].Normal.X * planes[i].Normal.X +
                    planes[i].Normal.Y * planes[i].Normal.Y +
                    planes[i].Normal.Z * planes[i].Normal.Z);
                
                planes[i] = new Plane(
                    planes[i].Normal / length,
                    planes[i].D / length
                );
            }
            
            return planes;
        }
        
        private static bool IsBoxOutsidePlane(Vector3 min, Vector3 max, Plane plane)
        {
            // Find the point of the box closest to the plane
            Vector3 p = new(
                plane.Normal.X > 0 ? min.X : max.X,
                plane.Normal.Y > 0 ? min.Y : max.Y,
                plane.Normal.Z > 0 ? min.Z : max.Z
            );
            
            // If this point is outside the plane, the entire box is outside
            return plane.Normal.X * p.X + plane.Normal.Y * p.Y + plane.Normal.Z * p.Z + plane.D < 0;
        }
    }
}