using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace DotNetANPR.Recognizer;

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

        // Create an SKBitmap to hold the histogram image
        var histogram = new SKBitmap(width + 20, height + 20);

        // Create an SKCanvas to draw on the SKBitmap
        using var canvas = new SKCanvas(histogram);

        // Set the background color to light gray
        var paint = new SKPaint
        {
            Color = SKColors.LightGray
        };

        // Draw the background rectangle
        var backRect = new SKRect(0, 0, width + 20, height + 20);
        canvas.DrawRect(backRect, paint);

        // Set the color to black for drawing text and lines
        paint.Color = SKColors.Black;

        // Draw the Y-axis labels and lines
        var colWidth = width / _patterns.Count;
        for (var ay = 0; ay <= 100; ay += 10)
        {
            var y = 15 + (int)(((100 - ay) / 100.0f) * (height - 20));
            canvas.DrawText(ay.ToString(), 3, y + 11, paint);
            canvas.DrawLine(25, y + 5, 35, y + 5, paint);
        }

        // Draw the Y-axis line
        canvas.DrawLine(35, 19, 35, height, paint);

        // Set the color to blue for drawing the rectangles and text
        paint.Color = SKColors.Blue;

        // Draw the bars and labels for each pattern
        for (var i = 0; i < _patterns.Count; i++)
        {
            var left = (i * colWidth) + 42;
            var top = height - (int)(_patterns[i].Cost * (height - 20));
            canvas.DrawRect(new SKRect(left, top, left + colWidth - 2, height), paint);
            canvas.DrawText(_patterns[i].Char + " ", left + 2, top - 8, paint);
        }

        return histogram;
    }
}
