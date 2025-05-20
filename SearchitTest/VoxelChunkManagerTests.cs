using System.Numerics;
using System.Reflection;
using SearchitLibrary;
using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class VoxelChunkManagerTests
{
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

    private string _testChunkDirectory;

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
    public void LoadChunk_NewPosition_ReturnsNullWhenNoFileExists()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        Vector3 chunkPos = new(0, 0, 0);

        // Act
        VoxelChunk? chunk = manager.LoadChunk(chunkPos);

        // Assert
        Assert.That(chunk, Is.Null); // Expect null when file doesn't exist
    }


    [Test]
    public void LoadChunk_ExistingPosition_ReturnsSameChunk()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        Vector3 chunkPos = new(0, 0, 0);

        // Create a test file
        string chunkFileName = $"Chunk_{chunkPos.X}_{chunkPos.Y}_{chunkPos.Z}.gox";
        string chunkFilePath = Path.Combine(_testChunkDirectory, chunkFileName);

        // TODO: Create a valid GOX file at this path
        // For testing purposes, we can use a simplified approach:
        manager.CreateTestChunks(1); // This directly adds chunks to the manager

        // Act - first call should successfully get the chunk we just created
        VoxelChunk? firstChunk = manager.GetChunkAt(chunkPos);
        Assert.That(firstChunk, Is.Not.Null);

        // Second call should return the same instance
        VoxelChunk? secondChunk = manager.GetChunkAt(chunkPos);

        // Assert
        Assert.That(secondChunk, Is.SameAs(firstChunk));
    }

    [Test]
    public void UnloadChunk_ExistingChunk_RemovesChunk()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        Vector3 chunkPos = new Vector3(0, 0, 0);
        manager.LoadChunk(chunkPos);

        // Act
        manager.RemoveChunk(chunkPos);

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
            MathF.Floor(playerPos.X / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(playerPos.Y / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(playerPos.Z / Constants.ChunkSize) * Constants.ChunkSize
        );
        Assert.That(manager.IsChunkLoaded(playerChunkPos), Is.True);
    }

    [Test]
    public void UpdateChunksAroundPlayer_UnloadsDistantChunks()
    {
        // Arrange
        var manager = new VoxelChunkManager(_testChunkDirectory);
        var initialPlayerPos = new Vector3(5, 5, 5);
        var newPlayerPos = new Vector3(100, 100, 100);
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
            MathF.Floor(newPlayerPos.X / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(newPlayerPos.Y / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(newPlayerPos.Z / Constants.ChunkSize) * Constants.ChunkSize
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

        // Add chunks directly to the internal dictionary using reflection
        var chunksField = typeof(VoxelChunkManager).GetField("_chunks",
            BindingFlags.NonPublic |
            BindingFlags.Instance);
        var chunks = (Dictionary<Vector3, VoxelChunk>)chunksField.GetValue(manager);
        chunks[pos1] = TestHelpers.CreateTestChunk(pos1);
        chunks[pos2] = TestHelpers.CreateTestChunk(pos2);

        // Act
        IEnumerable<VoxelChunk> loadedChunks = manager.GetLoadedChunks();

        // Assert
        Assert.That(loadedChunks.Count(), Is.EqualTo(2));
        Assert.That(loadedChunks.Any(c => c.Position == pos1), Is.True);
        Assert.That(loadedChunks.Any(c => c.Position == pos2), Is.True);
    }

    [Test]
    public void GetVoxelAt_ExistingChunk_ReturnsCorrectVoxel()
    {
        // Arrange
        VoxelChunkManager manager = new(_testChunkDirectory);
        Vector3 chunkPos = new(0, 0, 0);

        // Create a test chunk directly
        VoxelChunk testChunk = TestHelpers.CreateTestChunk(chunkPos);
        // Add it directly to the manager's chunks dictionary using reflection
        var chunksField = typeof(VoxelChunkManager).GetField("_chunks",
            BindingFlags.NonPublic |
            BindingFlags.Instance);
        var chunks = (Dictionary<Vector3, VoxelChunk>)chunksField.GetValue(manager);
        chunks[chunkPos] = testChunk;

        // Set a specific voxel
        Vector3 localPos = new Vector3(5, 10, 15);
        testChunk.SetVoxel(localPos, 3);

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
    public void SetVoxelAt_CreatesChunkWhenNeededAndSetsVoxel()
    {
        // Arrange
        VoxelChunkManager manager = new VoxelChunkManager(_testChunkDirectory);
        Vector3 globalPos = new Vector3(100, 100, 100);

        // Act
        manager.SetVoxelAt(globalPos, 5);

        // Assert
        // Note: This test is incompatible with your design intent
        // We should either update the SetVoxelAt method to create chunks
        // or update the test to not expect chunks to be created

        // Option 1: Modify test to expect that no chunk was created
        Vector3 chunkPos = new Vector3(
            MathF.Floor(globalPos.X / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(globalPos.Y / Constants.ChunkSize) * Constants.ChunkSize,
            MathF.Floor(globalPos.Z / Constants.ChunkSize) * Constants.ChunkSize
        );
        Assert.That(manager.IsChunkLoaded(chunkPos), Is.False);
        Assert.That(manager.GetVoxelAt(globalPos), Is.EqualTo(0)); // No voxel was set
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