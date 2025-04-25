using SearchitLibrary.Graphics;
using System.Numerics;

namespace SearchitTest;

public class VoxelTests
{
    [Test]
    public void Voxel_Constructor_ShouldCreateVoxelWithCorrectProperties()
    {
        // Arrange
        Vector3 position = new Vector3(1.0f, 2.0f, 3.0f);
        Vector3 color = new Vector3(0.5f, 0.6f, 0.7f);
        float size = 1.0f;

        // Act
        var voxel = new Voxel(position, color, size);

        // Assert
        Assert.That(voxel.Position, Is.EqualTo(position));
        Assert.That(voxel.Color, Is.EqualTo(color));
        Assert.That(voxel.Size, Is.EqualTo(size));
    }

    [Test]
    public void CreateBasicVoxel_ShouldCreateRedVoxelAtOrigin()
    {
        // Arrange & Act
        var voxel = Voxel.CreateBasicVoxel();

        // Assert
        Assert.That(voxel, Is.Not.Null);
        Assert.That(voxel.Position, Is.EqualTo(new Vector3(0.0f, 0.0f, 0.0f)));
        Assert.That(voxel.Color, Is.EqualTo(new Vector3(1.0f, 0.0f, 0.0f))); // Red
        Assert.That(voxel.Size, Is.EqualTo(1.0f));
    }

    [Test]
    public void GetVertices_ShouldReturnEightVerticesForCube()
    {
        // Arrange
        var voxel = new Voxel(new Vector3(0, 0, 0), new Vector3(1, 0, 0), 1.0f);

        // Act
        var vertices = voxel.GetVertices();

        // Assert
        Assert.That(vertices, Is.Not.Null);
        Assert.That(vertices.Length, Is.EqualTo(8));

        // Check for the expected corners of a cube with size 1 at origin
        Assert.That(vertices[0], Is.EqualTo(new Vector3(-0.5f, -0.5f, -0.5f)));
        Assert.That(vertices[1], Is.EqualTo(new Vector3(0.5f, -0.5f, -0.5f)));
        Assert.That(vertices[2], Is.EqualTo(new Vector3(-0.5f, 0.5f, -0.5f)));
        Assert.That(vertices[3], Is.EqualTo(new Vector3(0.5f, 0.5f, -0.5f)));
        Assert.That(vertices[4], Is.EqualTo(new Vector3(-0.5f, -0.5f, 0.5f)));
        Assert.That(vertices[5], Is.EqualTo(new Vector3(0.5f, -0.5f, 0.5f)));
        Assert.That(vertices[6], Is.EqualTo(new Vector3(-0.5f, 0.5f, 0.5f)));
        Assert.That(vertices[7], Is.EqualTo(new Vector3(0.5f, 0.5f, 0.5f)));
    }

    [Test]
    public void GetVertices_ShouldBePositionedCorrectly()
    {
        // Arrange
        var position = new Vector3(1.0f, 2.0f, 3.0f);
        var voxel = new Voxel(position, new Vector3(1, 0, 0), 1.0f);

        // Act
        var vertices = voxel.GetVertices();

        // Assert
        Assert.That(vertices, Is.Not.Null);
        Assert.That(vertices.Length, Is.EqualTo(8));

        // Check that all vertices are offset by the position
        for (int i = 0; i < vertices.Length; i++)
        {
            Assert.That(vertices[i].X, Is.InRange(position.X - 0.5f, position.X + 0.5f));
            Assert.That(vertices[i].Y, Is.InRange(position.Y - 0.5f, position.Y + 0.5f));
            Assert.That(vertices[i].Z, Is.InRange(position.Z - 0.5f, position.Z + 0.5f));
        }
    }

    [Test]
    public void GetIndices_ShouldReturn36Indices()
    {
        // Arrange
        var voxel = new Voxel(new Vector3(0, 0, 0), new Vector3(1, 0, 0), 1.0f);

        // Act
        var indices = voxel.GetIndices();

        // Assert
        Assert.That(indices, Is.Not.Null);
        Assert.That(indices.Length, Is.EqualTo(36)); // 12 triangles * 3 indices

        // Check that all indices are in the valid range (0-7) for the vertices
        foreach (var index in indices)
        {
            Assert.That(index, Is.InRange(0, 7));
        }

        // Check that we have exactly 12 triangles (6 faces, 2 triangles per face)
        Assert.That(indices.Length / 3, Is.EqualTo(12));
    }
}