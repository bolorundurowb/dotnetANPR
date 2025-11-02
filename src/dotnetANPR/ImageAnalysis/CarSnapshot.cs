using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Config;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis
{
    public class CarSnapshot : Photo
    {
        private readonly AppSettings _config;
        private readonly List<LicensePlateBand> _bands;

        public CarSnapshot(string filepath, AppSettings config) : base(filepath)
        {
            _config = config;
            _bands = new List<LicensePlateBand>();
        }

        public CarSnapshot(SKBitmap bitmap, AppSettings config) : base(bitmap)
        {
            _config = config;
            _bands = new List<LicensePlateBand>();
        }

        public List<LicensePlateBand> GetBands() => _bands;

        public void FindBands()
        {
            int graphRankFilter = _config.ImageAnalysis.CarSnapshotGraphRankFilter;
            double peakFoot = _config.ImageAnalysis.CarSnapshotGraphPeakFootConstant;
            double peakDiff = _config.ImageAnalysis.CarSnapshotGraphPeakDiffMultiplicationConstant;
            int numBands = _config.PlateCandidates.NumberOfBands;
            int margins = _config.ImageAnalysis.CarSnapshotDistributorMargins;

            var graph = new CarSnapshotGraph(this, graphRankFilter);
            graph.FindPeaks(peakFoot, peakDiff, 0);

            foreach (var peak in graph.Peaks.Take(numBands))
            {
                int y = peak.Left - margins;
                int height = peak.Right - peak.Left + 2 * margins;
                if (y < 0) y = 0;
                if (y + height > Height) height = Height - y;

                var bandRect = SKRectI.Create(0, y, Width, height);
                var bandBitmap = new SKBitmap(bandRect.Width, bandRect.Height, Info);
                _bitmap.ExtractSubset(bandBitmap, bandRect);
                _bands.Add(new LicensePlateBand(bandBitmap, _config));
            }
        }
    }
}