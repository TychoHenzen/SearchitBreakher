using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class VoxelVisibilityTests
{
    [SetUp]
    public void Setup()
    {
        _emptyChunk = new VoxelChunk(Vector3.Zero);
        _singleVoxelChunk = new VoxelChunk(Vector3.Zero);
        // Place a single voxel at (0,0,0)
        _singleVoxelChunk.SetVoxel(0, 0, 0, 1);
    }

    private VoxelChunk _emptyChunk;
    private VoxelChunk _singleVoxelChunk;

    [Test]
    public void CalculateVisibleFaces_EmptyChunk_ReturnsZero()
    {
        var faces = VoxelVisibility.CalculateVisibleFaces(_emptyChunk);
        Assert.That(faces, Is.Zero);
    }

    [Test]
    public void CalculateVisibleFaces_SingleVoxelAtCorner_ReturnsSix()
    {
        // A lone voxel should have all 6 faces visible
        var faces = VoxelVisibility.CalculateVisibleFaces(_singleVoxelChunk);
        Assert.That(faces, Is.EqualTo(6));
    }

    [Test]
    public void IsFaceVisible_NoNeighbor_ReturnsTrue()
    {
        // For the single voxel at (0,0,0), adjacent negative indices are out-of-bounds
        Assert.That(
            VoxelVisibility.IsFaceVisible(_singleVoxelChunk, Vector3.Zero, VoxelVisibility.FaceDirection.Front),
            Is.True);
        Assert.That(
            VoxelVisibility.IsFaceVisible(_singleVoxelChunk, Vector3.Zero, VoxelVisibility.FaceDirection.Back),
            Is.True);
        Assert.That(
            VoxelVisibility.IsFaceVisible(_singleVoxelChunk, Vector3.Zero, VoxelVisibility.FaceDirection.Left),
            Is.True);
        Assert.That(
            VoxelVisibility.IsFaceVisible(_singleVoxelChunk, Vector3.Zero, VoxelVisibility.FaceDirection.Right),
            Is.True);
        Assert.That(
            VoxelVisibility.IsFaceVisible(_singleVoxelChunk, Vector3.Zero, VoxelVisibility.FaceDirection.Top),
            Is.True);
        Assert.That(
            VoxelVisibility.IsFaceVisible(_singleVoxelChunk, Vector3.Zero,
                VoxelVisibility.FaceDirection.Bottom),
            Is.True);
    }
}