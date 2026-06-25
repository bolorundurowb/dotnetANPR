using dotnetANPR.Configuration;
using dotnetANPR.Intelligence;
using dotnetANPR.Pipeline;
using dotnetANPR.Recognizer;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class PlateScorerTests
{
    [TestMethod]
    public void Score_HigherWhenLowerClassificationCost()
    {
        var settings = CreateSettings();
        var scorer = new PlateScorer();

        var lowCost = new PlateCandidate
        {
            RecognizedPlate = CreatePlateWithCosts(10f, 12f),
            PlateWidthHeightRatio = 4.5f,
        };
        var highCost = new PlateCandidate
        {
            RecognizedPlate = CreatePlateWithCosts(80f, 90f),
            PlateWidthHeightRatio = 4.5f,
        };

        lowCost.Score = scorer.Score(lowCost, settings);
        highCost.Score = scorer.Score(highCost, settings);

        lowCost.Score.Must().BeGreaterThan(highCost.Score);
    }

    private static AnprSettings CreateSettings() => new Configurator().CreateSettings(new ResourceLocator());

    private static RecognizedPlate CreatePlateWithCosts(float cost1, float cost2)
    {
        var plate = new RecognizedPlate();
        var rc1 = new RecognizedCharacter();
        rc1.AddPattern(new RecognizedPattern('A', cost1));
        rc1.Sort(false);
        var rc2 = new RecognizedCharacter();
        rc2.AddPattern(new RecognizedPattern('B', cost2));
        rc2.Sort(false);
        plate.AddCharacter(rc1);
        plate.AddCharacter(rc2);
        return plate;
    }
}
