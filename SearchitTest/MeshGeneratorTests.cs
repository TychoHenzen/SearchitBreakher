using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class MeshGeneratorTests
{
    [Test]
    public void GenerateMeshForChunk_NullChunk_ReturnsEmptyMeshData()
    {
        // Act
        var meshData = MeshGenerator.GenerateMeshForChunk(null, Vector3.Zero);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(meshData.Positions, Is.Empty);
            Assert.That(meshData.Colors, Is.Empty);
            Assert.That(meshData.Indices, Is.Empty);
        });
    }

    [Test]
    public void GenerateMeshForChunk_EmptyChunk_ReturnsEmptyMeshData()
    {
        // Arrange
        var chunk = new VoxelChunk(Vector3.Zero);

        // Act
        var meshData = MeshGenerator.GenerateMeshForChunk(chunk, Vector3.Zero);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(meshData.Positions, Is.Empty);
            Assert.That(meshData.Colors, Is.Empty);
            Assert.That(meshData.Indices, Is.Empty);
        });
    }

    [Test]
    public void GenerateMeshForChunk_SingleVoxel_GeneratesCorrectFaceCount()
    {
        // Arrange: create a chunk with a single voxel at the origin
        var chunk = new VoxelChunk(Vector3.Zero);
        chunk.SetVoxel(0, 0, 0, 1);
        var faceColors = VoxelColorMap.GetFaceColors(1);

        // Act
        var meshData = MeshGenerator.GenerateMeshForChunk(chunk, Vector3.Zero);

        Assert.Multiple(() =>
        {
            // Assert: 6 faces * 4 vertices each = 24 vertices
            Assert.That(meshData.Positions, Has.Length.EqualTo(6 * 4));
            Assert.That(meshData.Colors, Has.Length.EqualTo(6 * 4));
            // 6 faces * 2 triangles * 3 indices = 36 indices
            Assert.That(meshData.Indices, Has.Length.EqualTo(6 * 6));
        });

        // Check first face color (Front) is applied to the first 4 vertices
        for (var i = 0; i < 4; i++) Assert.That(meshData.Colors[i], Is.EqualTo(faceColors[0]));

        // Check the first 6 indices for the front face
        int[] expectedFrontIndices = { 0, 1, 2, 2, 1, 3 };
        for (var i = 0; i < expectedFrontIndices.Length; i++)
            Assert.That(meshData.Indices[i], Is.EqualTo(expectedFrontIndices[i]));
    }

    [Test]
    public void GenerateMeshForChunk_SingleVoxel_ShadingUnchangedWhenCameraClose()
    {
        // Arrange: voxel at (1,1,1) with type 2, camera very close
        var chunk = new VoxelChunk(Vector3.Zero);
        chunk.SetVoxel(1, 1, 1, 2);
        var faceColors = VoxelColorMap.GetFaceColors(2);
        var cameraPos = new Vector3(1, 1, 1); // within MinDistance

        // Act
        var meshData = MeshGenerator.GenerateMeshForChunk(chunk, cameraPos);

        // Assert: shading factor = MaxShade (1.0), so colors unchanged
        Assert.That(meshData.Colors, Has.Length.EqualTo(meshData.Positions.Length));
        // Test a sample color from the front face
        Assert.That(meshData.Colors[0], Is.EqualTo(faceColors[0]));
    }
}