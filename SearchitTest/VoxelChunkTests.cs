using System.Numerics;
using SearchitLibrary;
using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class VoxelChunkTests
{
    [Test]
    public void Constructor_ValidArguments_CreatesChunk()
    {
        // Arrange
        Vector3 position = new Vector3(0, 0, 0);

        // Act
        VoxelChunk chunk = new VoxelChunk(position);

        // Assert
        Assert.That(chunk.Position, Is.EqualTo(position));
        Assert.That(Constants.VoxelCount, Is.EqualTo(Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize));
    }

    [Test]
    public void Constructor_WithVoxelData_CreatesChunkWithData()
    {
        // Arrange
        Vector3 position = new Vector3(32, 0, 0);
        byte[] voxelData = new byte[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];
        // Set a specific voxel
        int testIndex = Helpers.GetIndex(10, 10, 10);
        voxelData[testIndex] = 1;

        // Act
        VoxelChunk chunk = new VoxelChunk(position, voxelData);

        // Assert
        Assert.That(chunk.Position, Is.EqualTo(position));
        Assert.That(chunk.GetVoxel(10, 10, 10), Is.EqualTo(1));
    }

    [Test]
    public void GetVoxel_ValidCoordinates_ReturnsCorrectValue()
    {
        // Arrange
        VoxelChunk chunk = new VoxelChunk(Vector3.Zero);
        chunk.SetVoxel(5, 10, 15, 3);

        // Act
        byte result = chunk.GetVoxel(5, 10, 15);

        // Assert
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void GetVoxel_OutOfBoundsCoordinates_ReturnsZero()
    {
        // Arrange
        VoxelChunk chunk = new VoxelChunk(Vector3.Zero);

        // Act
        byte result = chunk.GetVoxel(-1, 0, 0);
        byte result2 = chunk.GetVoxel(0, Constants.ChunkSize, 0);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(result2, Is.EqualTo(0));
    }

    [Test]
    public void SetVoxel_ValidCoordinates_SetsVoxelValue()
    {
        // Arrange
        VoxelChunk chunk = new VoxelChunk(Vector3.Zero);

        // Act
        chunk.SetVoxel(20, 25, 30, 5);

        // Assert
        Assert.That(chunk.GetVoxel(20, 25, 30), Is.EqualTo(5));
    }

    [Test]
    public void SetVoxel_OutOfBoundsCoordinates_DoesNothing()
    {
        // Arrange
        VoxelChunk chunk = new VoxelChunk(Vector3.Zero);

        // Act - This should not throw an exception
        chunk.SetVoxel(-1, 0, 0, 5);
        chunk.SetVoxel(0, Constants.ChunkSize, 0, 5);

        // Assert - Nothing to assert, we're just making sure it doesn't throw
    }

    [Test]
    public void GetIndex_ValidCoordinates_ReturnsCorrectIndex()
    {
        // Arrange
        int x = 5, y = 10, z = 15;

        // Act
        int index = Helpers.GetIndex(x, y, z);

        // Assert
        // Expected: x * ChunkSize² + y * ChunkSize + z
        int expected = z * Constants.ChunkSize * Constants.ChunkSize + y * Constants.ChunkSize + x;
        Assert.That(index, Is.EqualTo(expected));
    }

    [Test]
    public void GetPosition_ValidIndex_ReturnsCorrectPosition()
    {
        // Arrange
        int x = 5, y = 10, z = 15;
        int index = Helpers.GetIndex(x, y, z);

        // Act
        var position = TestHelpers.GetPosition(index);

        // Assert
        Assert.That(position.X, Is.EqualTo(x));
        Assert.That(position.Y, Is.EqualTo(y));
        Assert.That(position.Z, Is.EqualTo(z));
    }

    [Test]
    public void CreateTestChunk_CreatesChunkWithExpectedPattern()
    {
        // Arrange
        Vector3 position = new Vector3(32, 0, 0);

        // Act
        VoxelChunk chunk = TestHelpers.CreateTestChunk(position);

        // Assert
        Assert.That(chunk.Position, Is.EqualTo(position));

        // Test a few positions that should be part of the outer shell
        int centerStart = Constants.ChunkSize / 4;
        int centerEnd = Constants.ChunkSize - centerStart;

        // Front face
        Assert.That(chunk.GetVoxel(centerStart, centerStart, centerStart), Is.EqualTo(1));
        // Back face
        Assert.That(chunk.GetVoxel(centerEnd - 1, centerStart, centerStart), Is.EqualTo(1));
        // Inside the shell (should be empty)
        Assert.That(chunk.GetVoxel(centerStart + 1, centerStart + 1, centerStart + 1), Is.EqualTo(0));
    }
}