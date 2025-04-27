using SearchitLibrary.Graphics;
using System.Numerics;

namespace SearchitTest;

public class VoxelShaderTests
{
    [Test]
    public void Constructor_Default_SetsDefaultValues()
    {
        // Arrange & Act
        var shader = new VoxelShader();
        
        // Assert
        Assert.That(shader.MinDistance, Is.EqualTo(2.0f));
        Assert.That(shader.MaxDistance, Is.EqualTo(20.0f));
        Assert.That(shader.MinShade, Is.EqualTo(0.5f));
        Assert.That(shader.MaxShade, Is.EqualTo(1.0f));
    }
    
    [Test]
    public void Constructor_WithParameters_SetsSpecifiedValues()
    {
        // Arrange
        float minDistance = 5.0f;
        float maxDistance = 30.0f;
        float minShade = 0.3f;
        float maxShade = 0.9f;
        
        // Act
        var shader = new VoxelShader(minDistance, maxDistance, minShade, maxShade);
        
        // Assert
        Assert.That(shader.MinDistance, Is.EqualTo(minDistance));
        Assert.That(shader.MaxDistance, Is.EqualTo(maxDistance));
        Assert.That(shader.MinShade, Is.EqualTo(minShade));
        Assert.That(shader.MaxShade, Is.EqualTo(maxShade));
    }
    
    [Test]
    public void CalculateShadeFactor_MinDistance_ReturnsMaxShade()
    {
        // Arrange
        var shader = new VoxelShader();
        
        // Act
        float result = shader.CalculateShadeFactor(shader.MinDistance);
        
        // Assert
        Assert.That(result, Is.EqualTo(shader.MaxShade));
    }
    
    [Test]
    public void CalculateShadeFactor_MaxDistance_ReturnsMinShade()
    {
        // Arrange
        var shader = new VoxelShader();
        
        // Act
        float result = shader.CalculateShadeFactor(shader.MaxDistance);
        
        // Assert
        Assert.That(result, Is.EqualTo(shader.MinShade));
    }
    
    [Test]
    public void CalculateShadeFactor_MidDistance_ReturnsInterpolatedShade()
    {
        // Arrange
        var shader = new VoxelShader();
        float midDistance = (shader.MinDistance + shader.MaxDistance) / 2.0f;
        float expectedShade = (shader.MinShade + shader.MaxShade) / 2.0f;
        
        // Act
        float result = shader.CalculateShadeFactor(midDistance);
        
        // Assert
        Assert.That(result, Is.EqualTo(expectedShade).Within(0.001f));
    }
    
    [Test]
    public void CalculateShadeFactor_LessThanMinDistance_ReturnsMaxShade()
    {
        // Arrange
        var shader = new VoxelShader();
        float distance = shader.MinDistance - 1.0f;
        
        // Act
        float result = shader.CalculateShadeFactor(distance);
        
        // Assert
        Assert.That(result, Is.EqualTo(shader.MaxShade));
    }
    
    [Test]
    public void CalculateShadeFactor_GreaterThanMaxDistance_ReturnsMinShade()
    {
        // Arrange
        var shader = new VoxelShader();
        float distance = shader.MaxDistance + 1.0f;
        
        // Act
        float result = shader.CalculateShadeFactor(distance);
        
        // Assert
        Assert.That(result, Is.EqualTo(shader.MinShade));
    }
    
    [Test]
    public void ApplyDistanceShading_CloseDistance_ReturnsBrightColor()
    {
        // Arrange
        var shader = new VoxelShader();
        Vector3 color = Vector3.One; // White color (1, 1, 1)
        Vector3 position = Vector3.Zero; // At origin
        Vector3 cameraPosition = new Vector3(0, 0, shader.MinDistance - 0.5f); // Closer than min distance
        
        // Act
        Vector3 result = shader.ApplyDistanceShading(color, position, cameraPosition);
        
        // Assert
        // Should be maximum brightness (not darkened)
        Assert.That(result, Is.EqualTo(Vector3.One)); 
    }
    
    [Test]
    public void ApplyDistanceShading_FarDistance_ReturnsDarkenedColor()
    {
        // Arrange
        var shader = new VoxelShader();
        Vector3 color = Vector3.One; // White color (1, 1, 1)
        Vector3 position = Vector3.Zero; // At origin
        Vector3 cameraPosition = new Vector3(0, 0, shader.MaxDistance + 1.0f); // Farther than max distance
        
        // Act
        Vector3 result = shader.ApplyDistanceShading(color, position, cameraPosition);
        
        // Assert
        // Should be darkened to MinShade
        Assert.That(result, Is.EqualTo(new Vector3(shader.MinShade, shader.MinShade, shader.MinShade)));
    }
    
    [Test]
    public void ApplyDistanceShading_PreservesColorRatios()
    {
        // Arrange
        var shader = new VoxelShader();
        Vector3 color = new Vector3(1.0f, 0.5f, 0.0f); // Orange color
        Vector3 position = Vector3.Zero; // At origin
        Vector3 cameraPosition = new Vector3(0, 0, shader.MaxDistance); // At max distance
        
        // Act
        Vector3 result = shader.ApplyDistanceShading(color, position, cameraPosition);
        
        // Assert
        // Colors should be darkened but maintain their ratios
        Assert.That(result.X / result.Y, Is.EqualTo(color.X / color.Y).Within(0.001f));
        Assert.That(result.X, Is.EqualTo(shader.MinShade));
        Assert.That(result.Y, Is.EqualTo(0.5f * shader.MinShade));
        Assert.That(result.Z, Is.EqualTo(0.0f));
    }
}
