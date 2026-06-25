using dotnetANPR.Configuration;
using dotnetANPR.Pipeline;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class PlateValidatorTests
{
    private static AnprSettings DefaultSettings() =>
        new Configurator().CreateSettings(new ResourceLocator());

    [TestMethod]
    public void IsCharacterCountValid_WithinRange_ReturnsTrue()
    {
        var settings = DefaultSettings();
        var validator = new PlateValidator();
        validator.IsCharacterCountValid(8, settings).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IsCharacterCountValid_AtMinimum_ReturnsTrue()
    {
        var settings = DefaultSettings();
        var validator = new PlateValidator();
        validator.IsCharacterCountValid((int)settings.IntelligenceMinimumChars, settings).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IsCharacterCountValid_AtMaximum_ReturnsTrue()
    {
        var settings = DefaultSettings();
        var validator = new PlateValidator();
        validator.IsCharacterCountValid((int)settings.IntelligenceMaximumChars, settings).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IsCharacterCountValid_BelowMinimum_ReturnsFalse()
    {
        var settings = DefaultSettings();
        var validator = new PlateValidator();
        validator.IsCharacterCountValid((int)settings.IntelligenceMinimumChars - 1, settings).Verify().ToBeFalse();
    }

    [TestMethod]
    public void IsCharacterCountValid_AboveMaximum_ReturnsFalse()
    {
        var settings = DefaultSettings();
        var validator = new PlateValidator();
        validator.IsCharacterCountValid((int)settings.IntelligenceMaximumChars + 1, settings).Verify().ToBeFalse();
    }

    [TestMethod]
    public void IsCharacterCountValid_Zero_ReturnsFalse()
    {
        var settings = DefaultSettings();
        var validator = new PlateValidator();
        validator.IsCharacterCountValid(0, settings).Verify().ToBeFalse();
    }
}

[TestClass]
public class CharacterValidatorTests
{
    private static AnprSettings DefaultSettings() =>
        new Configurator().CreateSettings(new ResourceLocator());

    [TestMethod]
    public void IsClassificationCostValid_BelowThreshold_ReturnsTrue()
    {
        var settings = DefaultSettings();
        var validator = new CharacterValidator();
        validator.IsClassificationCostValid(50f, settings).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IsClassificationCostValid_AtThreshold_ReturnsTrue()
    {
        var settings = DefaultSettings();
        var validator = new CharacterValidator();
        validator.IsClassificationCostValid(
            (float)settings.IntelligenceMaxSimilarityCostDispersion, settings).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IsClassificationCostValid_AboveThreshold_ReturnsFalse()
    {
        var settings = DefaultSettings();
        var validator = new CharacterValidator();
        validator.IsClassificationCostValid(
            (float)settings.IntelligenceMaxSimilarityCostDispersion + 1f, settings).Verify().ToBeFalse();
    }

    [TestMethod]
    public void IsClassificationCostValid_ZeroCost_ReturnsTrue()
    {
        var settings = DefaultSettings();
        var validator = new CharacterValidator();
        validator.IsClassificationCostValid(0f, settings).Verify().ToBeTrue();
    }
}
