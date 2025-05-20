using SearchitLibrary.Graphics;

namespace SearchitTest;

[TestFixture]
public class VoxelColorMapTests
{
    [Test]
    public void GetFaceColors_KnownTypes_ReturnsSixColors()
    {
        for (byte type = 0; type <= 6; type++)
        {
            var colors = VoxelColorMap.GetFaceColors(type);
            Assert.That(colors.Length, Is.EqualTo(6), $"Type {type} should have 6 face colors");
        }
    }

    [Test]
    public void GetFaceColor_MatchesGetFaceColors()
    {
        byte type = 3;
        var all = VoxelColorMap.GetFaceColors(type);
        foreach (VoxelVisibility.FaceDirection dir in Enum.GetValues(typeof(VoxelVisibility.FaceDirection)))
        {
            var faceColor = VoxelColorMap.GetFaceColor(type, dir);
            Assert.That(faceColor, Is.EqualTo(all[(int)dir]));
        }
    }

    [Test]
    public void GetFaceColors_UnknownType_FallsBackToType1()
    {
        byte unknown = 99;
        var fallback = VoxelColorMap.GetFaceColors(1);
        var result = VoxelColorMap.GetFaceColors(unknown);
        Assert.That(result, Is.EqualTo(fallback));
    }
}