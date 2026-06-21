using System;
using System.Collections.Generic;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Intelligence;
using dotnetANPR.Recognizer;

namespace dotnetANPR.Pipeline;

internal sealed class PlateCandidate
{
    public RecognizedPlate RecognizedPlate { get; init; } = new();
    public int BandIndex { get; init; }
    public int PlateIndex { get; init; }
    public double Score { get; set; }
    public float PlateWidthHeightRatio { get; init; }
    public string RawText => RecognizedPlate.ToString();
}

internal sealed class PlateValidator
{
    public bool IsPlateShapeValid(Plate plate, AnprSettings settings)
    {
        var ratio = plate.Width / (float)plate.Height;
        return ratio >= settings.IntelligenceMinPlateWidthHeightRatio &&
               ratio <= settings.IntelligenceMaxPlateWidthHeightRatio;
    }

    public bool IsCharacterCountValid(int count, AnprSettings settings) =>
        count >= settings.IntelligenceMinimumChars && count <= settings.IntelligenceMaximumChars;

    public bool IsWidthDispersionValid(Plate plate, List<Character> chars, AnprSettings settings) =>
        plate.CharactersWidthDispersion(chars) <= settings.IntelligenceMaxCharWidthDispersion;

    public bool HasMinimumRecognizedChars(RecognizedPlate plate, AnprSettings settings) =>
        plate.Characters.Count >= settings.IntelligenceMinimumChars;
}

internal sealed class CharacterValidator
{
    public bool IsValid(
        Character chr,
        Plate plate,
        float averageHeight,
        float averageContrast,
        float averageBrightness,
        float averageHue,
        float averageSaturation,
        AnprSettings settings)
    {
        var widthHeightRatio = chr.PieceWidth / (float)chr.PieceHeight;
        if (widthHeightRatio < settings.IntelligenceMinCharWidthHeightRatio ||
            widthHeightRatio > settings.IntelligenceMaxCharWidthHeightRatio)
            return false;

        if (chr.PositionInPlate is null)
            return false;

        if ((chr.PositionInPlate.LeftX < 2 || chr.PositionInPlate.RightX > plate.Width - 1) &&
            widthHeightRatio < settings.IntelligenceMinEdgeCharWidthHeightRatio)
            return false;

        if (Math.Abs(chr.StatisticAverageBrightness - averageBrightness) >
            settings.IntelligenceMaxBrightnessCostDispersion)
            return false;

        if (Math.Abs(chr.StatisticContrast - averageContrast) >
            settings.IntelligenceMaxContrastCostDispersion)
            return false;

        if (Math.Abs(chr.StatisticAverageHue - averageHue) >
            settings.IntelligenceMaxHueCostDispersion)
            return false;

        if (Math.Abs(chr.StatisticAverageSaturation - averageSaturation) >
            settings.IntelligenceMaxSaturationCostDispersion)
            return false;

        if ((chr.PieceHeight - averageHeight) / averageHeight <
            -settings.IntelligenceMaxHeightCostDispersion)
            return false;

        return true;
    }

    public bool IsClassificationCostValid(float cost, AnprSettings settings) =>
        cost <= settings.IntelligenceMaxSimilarityCostDispersion;
}

internal sealed class PlateScorer
{
    private const double IdealPlateRatio = 4.5;

    public double Score(PlateCandidate candidate, AnprSettings settings)
    {
        if (candidate.RecognizedPlate.Characters.Count == 0)
            return 0;

        double totalCost = 0;
        foreach (var chr in candidate.RecognizedPlate.Characters)
        {
            if (chr.Patterns is null || chr.Patterns.Count == 0)
                continue;
            totalCost += chr.Patterns[0].Cost;
        }

        var avgCost = totalCost / candidate.RecognizedPlate.Characters.Count;
        var costScore = Math.Max(0, 1.0 - avgCost / settings.IntelligenceMaxSimilarityCostDispersion);

        var ratioDistance = Math.Abs(candidate.PlateWidthHeightRatio - IdealPlateRatio);
        var ratioScore = Math.Max(0, 1.0 - ratioDistance / IdealPlateRatio);

        var charCount = candidate.RecognizedPlate.Characters.Count;
        var midChars = (settings.IntelligenceMinimumChars + settings.IntelligenceMaximumChars) / 2.0;
        var charSpan = settings.IntelligenceMaximumChars - settings.IntelligenceMinimumChars;
        var charScore = charSpan > 0
            ? Math.Max(0, 1.0 - Math.Abs(charCount - midChars) / charSpan)
            : 1.0;

        return costScore * 0.6 + ratioScore * 0.25 + charScore * 0.15;
    }
}
