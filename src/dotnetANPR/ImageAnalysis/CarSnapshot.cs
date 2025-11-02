using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Config;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

public class CarSnapshot : Photo
{
    private readonly AppSettings _config;
    private readonly List<LicensePlateBand> _bands;

    public CarSnapshot(string filepath, AppSettings config) : base(filepath)
    {
        _config = config;
        _bands = [];
    }

    public CarSnapshot(SKBitmap bitmap, AppSettings config) : base(bitmap)
    {
        _config = config;
        _bands = [];
    }

    public List<LicensePlateBand> GetBands() => _bands;

    public void FindBands()
    {
        var graphRankFilter = _config.ImageAnalysis.CarSnapshotGraphRankFilter;
        var peakFoot = _config.ImageAnalysis.CarSnapshotGraphPeakFootConstant;
        var peakDiff = _config.ImageAnalysis.CarSnapshotGraphPeakDiffMultiplicationConstant;
        var numBands = _config.PlateCandidates.NumberOfBands;
        var margins = _config.ImageAnalysis.CarSnapshotDistributorMargins;

        var graph = new CarSnapshotGraph(this, graphRankFilter);
        graph.FindPeaks(peakFoot, peakDiff, 0);

        foreach (var peak in graph.Peaks.Take(numBands))
        {
            var y = peak.Left - margins;
            var height = peak.Right - peak.Left + 2 * margins;
            if (y < 0) y = 0;
            if (y + height > Height) height = Height - y;

            var bandRect = SKRectI.Create(0, y, Width, height);
            var bandBitmap = new SKBitmap(bandRect.Width, bandRect.Height, Info.ColorType, Info.AlphaType, Info.ColorSpace);
            _bitmap.ExtractSubset(bandBitmap, bandRect);
            _bands.Add(new LicensePlateBand(bandBitmap, _config));
        }
    }
}