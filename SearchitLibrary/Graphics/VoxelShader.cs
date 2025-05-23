using System;
using System.Numerics;

namespace SearchitLibrary.Graphics;

/// <summary>
/// Provides shading functionality for voxels based on distance from camera.
/// </summary>
public class VoxelShader
{
    // Constants for distance-based shading - extremely dramatic settings for maximum visibility
    public float MinDistance { get; set; } = 2.0f;
    public float MaxDistance { get; set; } = 20.0f;
    public float MinShade { get; set; } = 0.5f; 
    public float MaxShade { get; set; } = 1.0f;

    /// <summary>
    /// Creates a new instance of the VoxelShader with default settings.
    /// </summary>
    public VoxelShader()
    {
    }

    /// <summary>
    /// Creates a new instance of the VoxelShader with custom settings.
    /// </summary>
    /// <param name="minDistance">Minimum distance at which maximum shading is applied</param>
    /// <param name="maxDistance">Maximum distance at which minimum shading is applied</param>
    /// <param name="minShade">Minimum shade factor (darkest, applied at max distance)</param>
    /// <param name="maxShade">Maximum shade factor (brightest, applied at min distance)</param>
    public VoxelShader(float minDistance, float maxDistance, float minShade, float maxShade)
    {
        MinDistance = minDistance;
        MaxDistance = maxDistance;
        MinShade = minShade;
        MaxShade = maxShade;
    }

    /// <summary>
    /// Calculates a shade factor based on the distance from the camera.
    /// </summary>
    /// <param name="distance">Distance from the camera to the vertex</param>
    /// <returns>A shade factor between MinShade and MaxShade</returns>
    public float CalculateShadeFactor(float distance)
    {
        // Linear interpolation between MinShade and MaxShade based on distance
        if (distance <= MinDistance)
            return MaxShade;
        if (distance >= MaxDistance)
            return MinShade;
            
        float t = (distance - MinDistance) / (MaxDistance - MinDistance);
        return MaxShade - t * (MaxShade - MinShade);
    }

    /// <summary>
    /// Applies distance-based shading to a color based on its position relative to the camera.
    /// </summary>
    /// <param name="color">The original color to shade</param>
    /// <param name="position">The position of the vertex</param>
    /// <param name="cameraPosition">The position of the camera</param>
    /// <returns>The shaded color</returns>
    public Vector3 ApplyDistanceShading(Vector3 color, Vector3 position, Vector3 cameraPosition)
    {
        float distance = Vector3.Distance(position, cameraPosition);
        float shadeFactor = CalculateShadeFactor(distance);
        
        // Apply shade factor to the color
        Vector3 shadedColor = color * shadeFactor;
        
        // Clamp color values to valid range
        shadedColor.X = Math.Clamp(shadedColor.X, 0.0f, 1.0f);
        shadedColor.Y = Math.Clamp(shadedColor.Y, 0.0f, 1.0f);
        shadedColor.Z = Math.Clamp(shadedColor.Z, 0.0f, 1.0f);
        
        return shadedColor;
    }
    
    /// <summary>
    /// Applies distance-based shading to an array of vertices based on their position and color.
    /// </summary>
    /// <param name="vertices">The vertices to shade</param>
    /// <param name="colors">The original colors of the vertices</param>
    /// <param name="cameraPosition">The position of the camera</param>
    /// <returns>An array of shaded colors</returns>
    public Vector3[] ApplyDistanceShadingBatch(Vector3[] vertices, Vector3[] colors, Vector3 cameraPosition)
    {
        if (vertices.Length != colors.Length)
        {
            throw new ArgumentException("Vertices and colors arrays must have the same length");
        }
        
        Vector3[] shadedColors = new Vector3[colors.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            shadedColors[i] = ApplyDistanceShading(colors[i], vertices[i], cameraPosition);
        }
        
        return shadedColors;
    }
}
