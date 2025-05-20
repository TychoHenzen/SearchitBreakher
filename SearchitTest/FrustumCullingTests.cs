using System.Numerics;
using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class FrustumCullingTests
{
    [SetUp]
    public void Setup()
    {
        // Use an orthographic projection for predictable frustum bounds
        var view = Matrix4x4.Identity;
        var proj = Matrix4x4.CreateOrthographic(10, 10, 1, 100);
        _viewProjection = view * proj;
    }

    private Matrix4x4 _viewProjection;

    [Test]
    public void ChunkInsideFrustum_ReturnsTrue()
    {
        var chunkPos = new Vector3(0, 0, 10);
        var visible = FrustumCulling.IsChunkVisible(chunkPos, 1f, _viewProjection);
        Assert.That(visible, Is.True);
    }

    [Test]
    public void ChunkCompletelyOutsideRight_ReturnsFalse()
    {
        var chunkPos = new Vector3(6, 0, 10);
        var visible = FrustumCulling.IsChunkVisible(chunkPos, 1f, _viewProjection);
        Assert.That(visible, Is.False);
    }

    [Test]
    public void ChunkPartiallyIntersectingBoundary_ReturnsTrue()
    {
        var chunkPos = new Vector3(4.5f, 0, 10);
        var visible = FrustumCulling.IsChunkVisible(chunkPos, 1f, _viewProjection);
        Assert.That(visible, Is.True);
    }

    [Test]
    public void ChunkBeyondFarPlane_ReturnsFalse()
    {
        var chunkPos = new Vector3(0, 0, 101);
        var visible = FrustumCulling.IsChunkVisible(chunkPos, 1f, _viewProjection);
        Assert.That(visible, Is.False);
    }

    [Test]
    public void ChunkBeforeNearPlane_ReturnsFalse()
    {
        var chunkPos = new Vector3(0, 0, 0);
        var visible = FrustumCulling.IsChunkVisible(chunkPos, 0.5f, _viewProjection);
        Assert.That(visible, Is.True);
    }
}