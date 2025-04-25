using System.IO;
using System.Text.Json;
using NUnit.Framework;
using SearchitLibrary;

namespace SearchitTest
{
    public class ConstantsTests
    {
        private string _testFilePath;
        
        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), "test_constants.json");
            
            // Clean up any existing test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
        
        [Test]
        public void Constants_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var constants = new Constants();
            
            // Assert
            Assert.That(constants.LookSpeed, Is.EqualTo(0.1f));
            Assert.That(constants.MoveSpeed, Is.EqualTo(0.1f));
        }
        
        [Test]
        public void SaveToFile_CreatesJsonFile()
        {
            // Arrange
            var constants = new Constants();
            
            // Act
            constants.SaveToFile(_testFilePath);
            
            // Assert
            Assert.That(File.Exists(_testFilePath), Is.True);
            
            string json = File.ReadAllText(_testFilePath);
            Assert.That(json, Is.Not.Empty);
            
            var deserializedConstants = JsonSerializer.Deserialize<Constants>(json);
            Assert.That(deserializedConstants, Is.Not.Null);
            Assert.That(deserializedConstants.LookSpeed, Is.EqualTo(constants.LookSpeed));
            Assert.That(deserializedConstants.MoveSpeed, Is.EqualTo(constants.MoveSpeed));
        }
        
        [Test]
        public void LoadFromFile_ReturnsCorrectValues()
        {
            // Arrange
            var originalConstants = new Constants 
            { 
                LookSpeed = 0.2f, 
                MoveSpeed = 0.3f 
            };
            originalConstants.SaveToFile(_testFilePath);
            
            // Act
            var loadedConstants = Constants.LoadFromFile(_testFilePath);
            
            // Assert
            Assert.That(loadedConstants, Is.Not.Null);
            Assert.That(loadedConstants.LookSpeed, Is.EqualTo(originalConstants.LookSpeed));
            Assert.That(loadedConstants.MoveSpeed, Is.EqualTo(originalConstants.MoveSpeed));
        }
        
        [Test]
        public void LoadFromFile_FileDoesNotExist_ReturnsDefaultConstants()
        {
            // Arrange
            string nonExistentFilePath = Path.Combine(Path.GetTempPath(), "non_existent_file.json");
            
            // Make sure the file doesn't exist
            if (File.Exists(nonExistentFilePath))
            {
                File.Delete(nonExistentFilePath);
            }
            
            // Act
            var constants = Constants.LoadFromFile(nonExistentFilePath);
            
            // Assert
            Assert.That(constants, Is.Not.Null);
            Assert.That(constants.LookSpeed, Is.EqualTo(0.1f));
            Assert.That(constants.MoveSpeed, Is.EqualTo(0.1f));
        }
    }
}