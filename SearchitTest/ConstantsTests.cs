// ConstantsTests.cs (Updated)
using System.IO;
using NUnit.Framework;
using SearchitLibrary;
using SearchitLibrary.IO;
using SearchitLibrary.Abstractions;

namespace SearchitTest
{
    public class ConstantsTests
    {
        private string _testFilePath;
        private IConstantProvider _provider;

        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), "test_constants.json");

            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            _provider = new JsonConstantProvider(_testFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Test]
        public void Constants_DefaultValues_AreCorrect()
        {
            var constants = new Constants();
            Assert.That(constants.LookSpeed, Is.EqualTo(0.1f));
            Assert.That(constants.MoveSpeed, Is.EqualTo(0.1f));
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesValues()
        {
            var constants = new Constants
            {
                LookSpeed = 0.25f,
                MoveSpeed = 0.5f
            };

            _provider.Save(constants);
            var loaded = _provider.Get();

            Assert.That(loaded.LookSpeed, Is.EqualTo(0.25f));
            Assert.That(loaded.MoveSpeed, Is.EqualTo(0.5f));
        }

        [Test]
        public void CachingConstantProvider_OnlySavesWhenChanged()
        {
            var inner = new JsonConstantProvider(_testFilePath);
            var caching = new CachingConstantProvider(inner);

            var initial = caching.Get();
            caching.Save(initial); // Should not trigger a write (but no assertion yet)

            var updated = new Constants
            {
                LookSpeed = initial.LookSpeed + 0.05f,
                MoveSpeed = initial.MoveSpeed
            };

            caching.Save(updated);
            var reloaded = inner.Get();
            Assert.That(reloaded.LookSpeed, Is.EqualTo(updated.LookSpeed));
        }
    }
}