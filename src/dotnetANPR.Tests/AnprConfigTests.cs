using System.Text.Json;
using DotNetANPR.Configuration;
using OmniAssert;
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

        config.Photo.AdaptiveThresholdingRadius.Verify().ToBe(7);
        config.Character.NormalizedWidth.Verify().ToBe(8);
        config.Character.NormalizedHeight.Verify().ToBe(13);
        config.Intelligence.ClassificationMethod.Verify().ToBe(0);
        config.Intelligence.NumberOfBands.Verify().ToBe(3);
        config.NeuralNetwork.MaxK.Verify().ToBe(8000);
    }

    [Fact]
    public void Instance_LoadsEmbeddedConfigJson()
    {
        var config = AnprConfig.Instance;

        config.Intelligence.SyntaxDescriptionFile.Verify().ToBe("Resources/syntax.xml");
        config.Character.NeuralNetworkPath.Verify().ToBe("Resources/neuralnetworks/network_avgres_813_map.xml");
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

            config.Photo.AdaptiveThresholdingRadius.Verify().ToBe(99);
            config.Verify().ToBe(AnprConfig.Instance);
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

        ReferenceEquals(first, second).Verify().ToBeFalse();
    }
}
