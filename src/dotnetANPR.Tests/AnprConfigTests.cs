using System.Text.Json;
using DotNetANPR.Configuration;
using Xunit;

namespace DotNetANPR.Tests;

public class AnprConfigTests
{
    public AnprConfigTests()
    {
        AnprConfig.Reset();
    }

    [Fact]
    public void Instance_LoadsDefaultValues()
    {
        var config = AnprConfig.Instance;

        Assert.Equal(7, config.Photo.AdaptiveThresholdingRadius);
        Assert.Equal(8, config.Character.NormalizedWidth);
        Assert.Equal(13, config.Character.NormalizedHeight);
        Assert.Equal(0, config.Intelligence.ClassificationMethod);
        Assert.Equal(3, config.Intelligence.NumberOfBands);
        Assert.Equal(8000, config.NeuralNetwork.MaxK);
    }

    [Fact]
    public void Instance_LoadsEmbeddedConfigJson()
    {
        var config = AnprConfig.Instance;

        Assert.Equal("Resources/syntax.xml", config.Intelligence.SyntaxDescriptionFile);
        Assert.Equal("Resources/neuralnetworks/network_avgres_813_map.xml", config.Character.NeuralNetworkPath);
    }

    [Fact]
    public void Load_ReplacesSingletonInstance()
    {
        var json = JsonSerializer.Serialize(new AnprConfig
        {
            Photo = { AdaptiveThresholdingRadius = 99 }
        });

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, json);

        try
        {
            var config = AnprConfig.Load(tempFile);

            Assert.Equal(99, config.Photo.AdaptiveThresholdingRadius);
            Assert.Same(config, AnprConfig.Instance);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Reset_ClearsSingletonInstance()
    {
        var first = AnprConfig.Instance;
        AnprConfig.Reset();
        var second = AnprConfig.Instance;

        Assert.NotSame(first, second);
    }
}
