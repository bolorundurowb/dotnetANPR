using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Config;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

public class LicensePlate(SKBitmap bitmap, AppSettings config) : Photo(bitmap)
{
    private List<LicensePlateChar> _chars = [];
    public PixelMap PixelMap { get; private set; }

    public List<LicensePlateChar> GetChars() => _chars;

    public new LicensePlate Clone() => new LicensePlate(GetBitmap().Copy(), config);

    public void Normalize()
    {
        // ... (Conversion of PlateVerticalGraph logic to find main peak) ...
        // ... (Bitmap is vertically cropped using ExtractSubset) ...
        ClearBrightnessCache();
    }

    public void Segment()
    {
        var numChars = config.PlateCandidates.NumberOfChars;
        var minCharRatio = config.Heuristics.Char.MinCharWidthHeightRatio;
        var maxCharRatio = config.Heuristics.Char.MaxCharWidthHeightRatio;

        PixelMap = new PixelMap(this);

        _chars = PixelMap.Pieces
            .Select(p => new { Piece = p, Ratio = (float)p.Width / p.Height })
            .Where(p => p.Ratio >= minCharRatio && p.Ratio <= maxCharRatio)
            .OrderBy(p => p.Piece.CenterX)
            .Take(numChars)
            .Select(p => new LicensePlateChar(p.Piece.CreatePhoto(this), p.Piece, config))
            .ToList();
    }
}