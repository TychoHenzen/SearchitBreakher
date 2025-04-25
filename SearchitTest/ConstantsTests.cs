using System.Text.Json;
using System.IO;
using SearchitLibrary;

namespace SearchitTest;

public class ConstantsTests
{
    private const string TestJsonFilePath = "test_constants.json";

    [SetUp]
    public void Setup()
    {
        // Ensure we start with a clean state for each test
        if (File.Exists(TestJsonFilePath))
        {
            File.Delete(TestJsonFilePath);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        // Clean up after tests
        if (File.Exists(TestJsonFilePath))
        {
            File.Delete(TestJsonFilePath);
        }
    }

    [Test]
    public void SerializeToJson_ShouldCreateValidJsonFile()
    {
        // Arrange
        var constants = new Constants
        {
            WindowWidth = 800,
            WindowHeight = 600,
            GameTitle = "SearchitBreakher",
            FontSize = 12
        };

        // Act
        constants.SerializeToJson(TestJsonFilePath);

        // Assert
        Assert.That(File.Exists(TestJsonFilePath), Is.True);
        var jsonContent = File.ReadAllText(TestJsonFilePath);
        Assert.That(jsonContent, Does.Contain("WindowWidth"));
        Assert.That(jsonContent, Does.Contain("WindowHeight"));
        Assert.That(jsonContent, Does.Contain("GameTitle"));
        Assert.That(jsonContent, Does.Contain("FontSize"));
    }

    [Test]
    public void DeserializeFromJson_ShouldLoadCorrectValues()
    {
        // Arrange
        var originalConstants = new Constants
        {
            WindowWidth = 1024,
            WindowHeight = 768,
            GameTitle = "Test Game",
            FontSize = 16
        };
        originalConstants.SerializeToJson(TestJsonFilePath);

        // Act
        var loadedConstants = Constants.DeserializeFromJson(TestJsonFilePath);

        // Assert
        Assert.That(loadedConstants, Is.Not.Null);
        Assert.That(loadedConstants.WindowWidth, Is.EqualTo(1024));
        Assert.That(loadedConstants.WindowHeight, Is.EqualTo(768));
        Assert.That(loadedConstants.GameTitle, Is.EqualTo("Test Game"));
        Assert.That(loadedConstants.FontSize, Is.EqualTo(16));
    }

    [Test]
    public void DeserializeFromJson_ShouldReturnDefaultWhenFileNotFound()
    {
        // Arrange
        var nonExistentPath = "non_existent_file.json";
        
        // Act
        var constants = Constants.DeserializeFromJson(nonExistentPath);
        
        // Assert
        Assert.That(constants, Is.Not.Null);
        Assert.That(constants.WindowWidth, Is.EqualTo(Constants.DefaultWindowWidth));
        Assert.That(constants.WindowHeight, Is.EqualTo(Constants.DefaultWindowHeight));
        Assert.That(constants.GameTitle, Is.EqualTo(Constants.DefaultGameTitle));
        Assert.That(constants.FontSize, Is.EqualTo(Constants.DefaultFontSize));
    }

    [Test]
    public void Default_ShouldReturnDefaultValues()
    {
        // Act
        var defaultConstants = Constants.Default;
        
        // Assert
        Assert.That(defaultConstants, Is.Not.Null);
        Assert.That(defaultConstants.WindowWidth, Is.EqualTo(Constants.DefaultWindowWidth));
        Assert.That(defaultConstants.WindowHeight, Is.EqualTo(Constants.DefaultWindowHeight));
        Assert.That(defaultConstants.GameTitle, Is.EqualTo(Constants.DefaultGameTitle));
        Assert.That(defaultConstants.FontSize, Is.EqualTo(Constants.DefaultFontSize));
    }
}