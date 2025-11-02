using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Config;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;

namespace DotNetANPR.Intelligence
{
    public class AnprEngine
    {
        private readonly AppSettings _config;
        private readonly ICharacterRecognizer _recognizer;
        private readonly SyntaxParser _parser;

        public AnprEngine(AppSettings config, ICharacterRecognizer recognizer, SyntaxParser parser)
        {
            _config = config;
            _recognizer = recognizer;
            _parser = parser;
        }

        public RecognizedPlate Recognize(CarSnapshot snapshot)
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
                            if (_config.ImageAnalysis.SkewDetection != 0)
                            {
                                var hough = plate.GetHoughTransformation();
                                var line = hough.GetBestLine();
                                if (line.AngleDegrees != 0)
                                {
                                    plate.Rotate(line.AngleDegrees);
                                }
                            }

                            plate.Normalize(); // Vertical crop

                            float ratio = (float)plate.Width / plate.Height;
                            if (ratio < _config.Heuristics.Plate.MinPlateWidthHeightRatio ||
                                ratio > _config.Heuristics.Plate.MaxPlateWidthHeightRatio)
                            {
                                continue;
                            }

                            plate.Segment();
                            var chars = plate.GetChars();

                            if (chars.Count < _config.Heuristics.Plate.MinimumChars ||
                                chars.Count > _config.Heuristics.Plate.MaximumChars)
                            {
                                continue;
                            }

                            var recognizedChars = chars.Select(c => _recognizer.Recognize(c)).ToList();
                            var finalPlate = _parser.Parse(recognizedChars, "default");
                            
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
}