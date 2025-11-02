using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Config;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;

namespace DotNetANPR.Intelligence;

public class AnprEngine(AppSettings config, ICharacterRecognizer recognizer, SyntaxParser parser)
{
    public RecognizedPlate? Recognize(CarSnapshot snapshot)
    {
        var allPlates = new List<RecognizedPlate>();

        try
        {
            snapshot.FindBands();

            foreach (var band in snapshot.GetBands())
            {
                band.FindPlates();
                foreach (var plate in band.GetPlates())
                {
                    try
                    {
                        if (config.ImageAnalysis.SkewDetection != 0)
                        {
                            var hough = plate.GetHoughTransformation();
                            var line = hough.GetBestLine();
                            if (line.AngleDegrees != 0)
                            {
                                plate.Rotate(line.AngleDegrees);
                            }
                        }

                        plate.Normalize(); // Vertical crop

                        var ratio = (float)plate.Width / plate.Height;
                        if (ratio < config.Heuristics.Plate.MinPlateWidthHeightRatio ||
                            ratio > config.Heuristics.Plate.MaxPlateWidthHeightRatio)
                        {
                            continue;
                        }

                        plate.Segment();
                        var chars = plate.GetChars();

                        if (chars.Count < config.Heuristics.Plate.MinimumChars ||
                            chars.Count > config.Heuristics.Plate.MaximumChars)
                        {
                            continue;
                        }

                        var recognizedChars = chars.Select(c => recognizer.Recognize(c)).ToList();
                        var finalPlate = parser.Parse(recognizedChars, "default");

                        allPlates.Add(new RecognizedPlate(finalPlate.Text, finalPlate.Confidence, plate.Clone()));
                    }
                    finally
                    {
                        plate.GetChars().ForEach(c => c.Dispose());
                        plate.Dispose();
                    }
                }
                band.Dispose();
            }
        }
        finally
        {
            snapshot.Dispose();
        }

        // Return the best plate found
        return allPlates.OrderByDescending(p => p.Confidence).FirstOrDefault();
    }
}