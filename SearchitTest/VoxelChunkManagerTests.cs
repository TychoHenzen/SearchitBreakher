using NUnit.Framework;
using SearchitLibrary.Graphics;
using System.IO;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;

namespace SearchitTest;

[TestFixture]
public class VoxelChunkManagerTests
{
    private string _testChunkDirectory;
    
    [SetUp]
    public void Setup()
    {
        // Create a temporary directory for test chunks
        _testChunkDirectory = Path.Combine(Path.GetTempPath(), "TestChunks_" + Path.GetRandomFileName());
        Directory.CreateDirectory(_testChunkDirectory);
    }
    
    [TearDown]
    public void Cleanup()
    {
        // Clean up the test directory
        if (Directory.Exists(_testChunkDirectory))
        {
            try
            {
                Directory.Delete(_testChunkDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
    
    [Test]
    public void Constructor_CreatesManagerWithEmptyChunks()
    {
        // Arrange & Act
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        
        // Assert
        Assert.That(manager.LoadedChunkCount, Is.EqualTo(0));
        Assert.That(Directory.Exists(_testChunkDirectory), Is.True);
    }
    
    [Test]
    public void LoadChunk_NewPosition_CreatesAndReturnsChunk()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        Vector3 chunkPos = new Vector3(0, 0, 0);
        
        // Act
        VoxelChunk? chunk = manager.LoadChunk(chunkPos);
        
        // Assert
        Assert.That(chunk, Is.Not.Null);
        Assert.That(chunk!.Position, Is.EqualTo(chunkPos));
        Assert.That(manager.LoadedChunkCount, Is.EqualTo(1));
    }
    
    [Test]
    public void LoadChunk_ExistingPosition_ReturnsSameChunk()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        Vector3 chunkPos = new(0, 0, 0);
        VoxelChunk? firstChunk = manager.LoadChunk(chunkPos);
        Assert.That(firstChunk, Is.Not.Null);
        
        // Act
        VoxelChunk? secondChunk = manager.LoadChunk(chunkPos);
        
        // Assert
        Assert.That(secondChunk, Is.SameAs(firstChunk));
        Assert.That(manager.LoadedChunkCount, Is.EqualTo(1));
    }
    
    [Test]
    public void UnloadChunk_ExistingChunk_RemovesChunk()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        Vector3 chunkPos = new Vector3(0, 0, 0);
        manager.LoadChunk(chunkPos);
        
        // Act
        manager.UnloadChunk(chunkPos);
        
        // Assert
        Assert.That(manager.LoadedChunkCount, Is.EqualTo(0));
        Assert.That(manager.IsChunkLoaded(chunkPos), Is.False);
    }
    
    [Test]
    public void UpdateChunksAroundPlayer_LoadsNearbyChunks()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        Vector3 playerPos = new Vector3(5, 5, 5);
        int loadRadius = 1;
        
        // Act
        manager.UpdateChunksAroundPlayer(playerPos, loadRadius);
        
        // Assert
        // Should load a 3x3x3 cube of chunks around the player
        Assert.That(manager.LoadedChunkCount, Is.EqualTo(27));
        
        // Check that the chunk the player is in is loaded
        Vector3 playerChunkPos = new Vector3(
            MathF.Floor(playerPos.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(playerPos.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(playerPos.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        Assert.That(manager.IsChunkLoaded(playerChunkPos), Is.True);
    }
    
    [Test]
    public void UpdateChunksAroundPlayer_UnloadsDistantChunks()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        Vector3 initialPlayerPos = new Vector3(5, 5, 5);
        Vector3 newPlayerPos = new Vector3(100, 100, 100);
        int loadRadius = 1;
        
        // Load chunks around initial position
        manager.UpdateChunksAroundPlayer(initialPlayerPos, loadRadius);
        
        // Get a chunk that should be unloaded after the move
        Vector3 initialChunkPos = new Vector3(0, 0, 0);
        Assert.That(manager.IsChunkLoaded(initialChunkPos), Is.True);
        
        // Act
        manager.UpdateChunksAroundPlayer(newPlayerPos, loadRadius);
        
        // Assert
        // The initial chunk should be unloaded
        Assert.That(manager.IsChunkLoaded(initialChunkPos), Is.False);
        
        // New chunks around the player should be loaded
        Vector3 newChunkPos = new(
            MathF.Floor(newPlayerPos.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(newPlayerPos.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(newPlayerPos.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        Assert.That(manager.IsChunkLoaded(newChunkPos), Is.True);
    }
    
    [Test]
    public void GetLoadedChunks_ReturnsAllLoadedChunks()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        Vector3 pos1 = new(0, 0, 0);
        Vector3 pos2 = new(32, 0, 0);
        manager.LoadChunk(pos1);
        manager.LoadChunk(pos2);
        
        // Act
        IEnumerable<VoxelChunk> chunks = manager.GetLoadedChunks();
        
        // Assert
        Assert.That(chunks.Count(), Is.EqualTo(2));
        Assert.That(chunks.Any(c => c.Position == pos1), Is.True);
        Assert.That(chunks.Any(c => c.Position == pos2), Is.True);
    }
    
    [Test]
    public void GetVoxelAt_ExistingChunk_ReturnsCorrectVoxel()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        Vector3 chunkPos = new(0, 0, 0);
        VoxelChunk? chunk = manager.LoadChunk(chunkPos);
        
        // Ensure chunk is not null
        Assert.That(chunk, Is.Not.Null);
        
        // Set a specific voxel
        Vector3 localPos = new Vector3(5, 10, 15);
        chunk!.SetVoxel(localPos, 3);
        
        // Calculate the global position
        Vector3 globalPos = chunkPos + localPos;
        
        // Act
        byte voxelValue = manager.GetVoxelAt(globalPos);
        
        // Assert
        Assert.That(voxelValue, Is.EqualTo(3));
    }
    
    [Test]
    public void GetVoxelAt_UnloadedChunk_ReturnsZero()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        
        // Position in an unloaded chunk
        Vector3 globalPos = new Vector3(100, 100, 100);
        
        // Act
        byte voxelValue = manager.GetVoxelAt(globalPos);
        
        // Assert
        Assert.That(voxelValue, Is.EqualTo(0));
    }
    
    [Test]
    public void SetVoxelAt_LoadsChunkAndSetsVoxel()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        
        // Position in an unloaded chunk
        Vector3 globalPos = new Vector3(100, 100, 100);
        
        // Act
        manager.SetVoxelAt(globalPos, 5);
        
        // Assert
        // The chunk should be loaded
        Vector3 chunkPos = new Vector3(
            MathF.Floor(globalPos.X / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(globalPos.Y / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize,
            MathF.Floor(globalPos.Z / VoxelChunk.ChunkSize) * VoxelChunk.ChunkSize
        );
        Assert.That(manager.IsChunkLoaded(chunkPos), Is.True);
        
        // The voxel value should be set
        Assert.That(manager.GetVoxelAt(globalPos), Is.EqualTo(5));
    }
    
    [Test]
    public void CreateTestChunks_CreatesSpecifiedNumberOfChunks()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        int radius = 1;
        
        // Act
        manager.CreateTestChunks(radius);
        
        // Assert
        // Should create a (2*radius+1)^3 cube of chunks centered at origin
        int expectedCount = (int)Math.Pow(2 * radius + 1, 3);
        Assert.That(manager.LoadedChunkCount, Is.EqualTo(expectedCount));
    }
}
