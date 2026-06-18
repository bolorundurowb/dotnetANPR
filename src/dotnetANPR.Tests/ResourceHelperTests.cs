using System.Linq;
using DotNetANPR.Utilities;
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
        Assert.True(ResourceHelper.Exists(path));
    }

    [Fact]
    public void Exists_ReturnsFalse_ForMissingResource()
    {
        Assert.False(ResourceHelper.Exists("Resources/does-not-exist.txt"));
    }

    [Fact]
    public void OpenStream_ReturnsReadableStream_ForEmbeddedConfig()
    {
        using var stream = ResourceHelper.OpenStream("Resources/config.json");

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void ReadText_ReturnsConfigContent_ForEmbeddedConfig()
    {
        var text = ResourceHelper.ReadText("Resources/config.json");

        Assert.NotNull(text);
        Assert.Contains("\"photo\"", text);
    }

    [Fact]
    public void Enumerate_ReturnsAlphabetImages_ForEmbeddedAlphabet()
    {
        var allNames = typeof(ResourceHelper).Assembly.GetManifestResourceNames();
        Assert.Contains(allNames, n => n.Contains("alphabet_8x13"));

        var images = ResourceHelper.Enumerate("Resources/alphabets/alphabet_8x13", ".jpg");

        Assert.NotEmpty(images);
        Assert.Contains(images, i => i.EndsWith("a_8x13.jpg"));
        Assert.Contains(images, i => i.EndsWith("0_8x13.jpg"));
    }

    [Fact]
    public void Enumerate_FallsBackToFileSystem_WhenNoEmbeddedResourcesMatch()
    {
        var images = ResourceHelper.Enumerate(Path.GetTempPath(), ".not-a-real-extension");

        Assert.Empty(images);
    }
}
