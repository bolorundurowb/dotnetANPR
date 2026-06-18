using System.IO;
using System.Linq;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Utilities;
using OmniAssert;
using Xunit;

namespace DotNetANPR.Tests;

public class CharacterTests
{
    [Fact]
    public void AlphabetList_ReturnsEmbeddedResources_ForDefaultAlphabetPath()
    {
        var list = Character.AlphabetList("Resources/alphabets/alphabet_8x13");

        list.Verify().NotToBeEmpty();
        list.Verify().AllSatisfy(path => ResourceHelper.Exists(path));
    }

    [Fact]
    public void AlphabetList_ReturnsFiles_WhenDirectoryExistsOnDisk()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "alphabet_test");
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "a_test.jpg"), string.Empty);
            File.WriteAllText(Path.Combine(tempDir, "b_test.jpg"), string.Empty);

            var list = Character.AlphabetList(tempDir);

            list.Verify().ToHaveCount(2);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_LoadsFromEmbeddedStream()
    {
        using var stream = ResourceHelper.OpenStream("Resources/alphabets/alphabet_8x13/a_8x13.jpg");

        stream.Verify().NotToBeNull();
        using var character = new Character(stream!);

        character.Width.Verify().ToBeGreaterThan(0);
        character.Height.Verify().ToBeGreaterThan(0);
    }
}
