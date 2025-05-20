using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class VoxelFaceCalculatorTests
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
        var faces = VoxelFaceCalculator.CalculateVisibleFaces(_emptyChunk);
        Assert.That(faces, Is.Zero);
    }

    [Test]
    public void CalculateVisibleFaces_SingleVoxelAtCorner_ReturnsSix()
    {
        // A lone voxel should have all 6 faces visible
        var faces = VoxelFaceCalculator.CalculateVisibleFaces(_singleVoxelChunk);
        Assert.That(faces, Is.EqualTo(6));
    }

    [Test]
    public void IsFaceVisible_NoNeighbor_ReturnsTrue()
    {
        // For the single voxel at (0,0,0), adjacent negative indices are out-of-bounds
        Assert.That(
            VoxelFaceCalculator.IsFaceVisible(_singleVoxelChunk, 0, 0, 0, VoxelFaceCalculator.FaceDirection.Front),
            Is.True);
        Assert.That(
            VoxelFaceCalculator.IsFaceVisible(_singleVoxelChunk, 0, 0, 0, VoxelFaceCalculator.FaceDirection.Back),
            Is.True);
        Assert.That(
            VoxelFaceCalculator.IsFaceVisible(_singleVoxelChunk, 0, 0, 0, VoxelFaceCalculator.FaceDirection.Left),
            Is.True);
        Assert.That(
            VoxelFaceCalculator.IsFaceVisible(_singleVoxelChunk, 0, 0, 0, VoxelFaceCalculator.FaceDirection.Right),
            Is.True);
        Assert.That(
            VoxelFaceCalculator.IsFaceVisible(_singleVoxelChunk, 0, 0, 0, VoxelFaceCalculator.FaceDirection.Top),
            Is.True);
        Assert.That(
            VoxelFaceCalculator.IsFaceVisible(_singleVoxelChunk, 0, 0, 0, VoxelFaceCalculator.FaceDirection.Bottom),
            Is.True);
    }
}