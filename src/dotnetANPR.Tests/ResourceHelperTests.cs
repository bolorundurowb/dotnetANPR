using System.Linq;
using DotNetANPR.Utilities;
using OmniAssert;
using Xunit;

namespace DotNetANPR.Tests;

public class ResourceHelperTests
{
    [Theory]
    [InlineData("Resources/config.json")]
    [InlineData("Resources/syntax.xml")]
    [InlineData("Resources/neuralnetworks/network_avgres_813_map.xml")]
    public void Exists_ReturnsTrue_ForEmbeddedResources(string path)
    {
        ResourceHelper.Exists(path).Verify().ToBeTrue();
    }

    [Fact]
    public void Exists_ReturnsFalse_ForMissingResource()
    {
        ResourceHelper.Exists("Resources/does-not-exist.txt").Verify().ToBeFalse();
    }

    [Fact]
    public void OpenStream_ReturnsReadableStream_ForEmbeddedConfig()
    {
        using var stream = ResourceHelper.OpenStream("Resources/config.json");

        stream.Verify().NotToBeNull();
        stream!.Length.Verify().ToBeGreaterThan(0);
    }

    [Fact]
    public void ReadText_ReturnsConfigContent_ForEmbeddedConfig()
    {
        var text = ResourceHelper.ReadText("Resources/config.json");

        text.Verify().NotToBeNull();
        text!.Verify().ToContain("\"photo\"");
    }

    [Fact]
    public void Enumerate_ReturnsAlphabetImages_ForEmbeddedAlphabet()
    {
        var allNames = typeof(ResourceHelper).Assembly.GetManifestResourceNames();
        allNames.Verify().ToContain(n => n.Contains("alphabet_8x13"));

        var images = ResourceHelper.Enumerate("Resources/alphabets/alphabet_8x13", ".jpg");

        images.Verify().NotToBeEmpty();
        images.Verify().ToContain(i => i.EndsWith("a_8x13.jpg"));
        images.Verify().ToContain(i => i.EndsWith("0_8x13.jpg"));
    }

    [Fact]
    public void Enumerate_FallsBackToFileSystem_WhenNoEmbeddedResourcesMatch()
    {
        var images = ResourceHelper.Enumerate(Path.GetTempPath(), ".not-a-real-extension");

        images.Verify().ToBeEmpty();
    }
}
