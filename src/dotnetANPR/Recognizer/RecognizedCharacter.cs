using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace dotnetANPR.Recognizer;

public class RecognizedCharacter
{
    private readonly List<RecognizedPattern> _patterns = [];

    public bool IsSorted { get; private set; }

    public List<RecognizedPattern>? Patterns => !IsSorted ? null : _patterns;

    public void AddPattern(RecognizedPattern pattern) => _patterns.Add(pattern);

    public RecognizedPattern? Pattern(int index) => Patterns?.ElementAtOrDefault(index);

    public void Sort(bool sortDesc)
    {
        if (IsSorted)
            return;

        _patterns.Sort(new RecognizedPatternComparer(sortDesc));
        IsSorted = true;
    }

    public SKBitmap Render()
    {
        var width = 500;
        var height = 200;
        var histogram = new SKBitmap(width + 20, height + 20);

        using var canvas = new SKCanvas(histogram);

        // Fill background with light gray
        canvas.Clear(new SKColor(211, 211, 211)); // LightGray

        // Draw background rectangle
        using var backPaint = new SKPaint
        {
            Color = new SKColor(211, 211, 211), // LightGray
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(0, 0, width + 20, height + 20, backPaint);

        // Draw border rectangle
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        canvas.DrawRect(0, 0, width + 20, height + 20, borderPaint);

        // Draw axis labels using SKFont
        using var font = new SKFont();
        font.Size = 12;

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        canvas.DrawText("0", 3, height + 15, font, textPaint);

        var colWidth = _patterns.Count > 0 ? width / _patterns.Count : 1;

        // Draw Y-axis tick marks and labels
        for (var ay = 0; ay <= 100; ay += 10)
        {
            var y = 15 + (int)((100 - ay) / 100.0f * (height - 20));
            canvas.DrawText(ay.ToString(), 3, y + 20, font, textPaint);

            using var linePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawLine(25, y + 5, 35, y + 5, linePaint);
        }

        // Draw Y-axis line
        using var yAxisPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        canvas.DrawLine(35, 19, 35, height, yAxisPaint);
        canvas.DrawText("100", 3, 35, font, textPaint);

        // Draw bars for each pattern
        using var barPaint = new SKPaint
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        for (var i = 0; i < _patterns.Count; i++)
        {
            var left = i * colWidth + 42;
            var top = height - (int)(_patterns[i].Cost * (height - 20));
            canvas.DrawRect(left, top, colWidth - 2, height - top, barPaint);
            canvas.DrawText(_patterns[i].Char + " ", left + 2, top - 8, font, textPaint);
        }

        return histogram;
    }
}
