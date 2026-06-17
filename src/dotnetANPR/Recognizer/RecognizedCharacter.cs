using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace DotNetANPR.Recognizer;

/// <summary>
/// Holds a list of <see cref="RecognizedPattern"/> results for a single character position,
/// representing all candidate interpretations with their costs. Must be sorted before
/// accessing patterns.
/// </summary>
public class RecognizedCharacter
{
    private readonly List<RecognizedPattern> _patterns = new();

    /// <summary>
    /// Gets a value indicating whether the patterns have been sorted.
    /// </summary>
    public bool IsSorted { get; private set; }

    /// <summary>
    /// Gets the list of recognized patterns if sorted; otherwise <c>null</c>.
    /// </summary>
    public List<RecognizedPattern>? Patterns => !IsSorted ? null : _patterns;

    /// <summary>
    /// Adds a recognition result to this character's list of candidates.
    /// </summary>
    /// <param name="pattern">The pattern to add.</param>
    public void AddPattern(RecognizedPattern pattern) => _patterns.Add(pattern);

    /// <summary>
    /// Gets the pattern at the specified index, or <c>null</c> if the list is not sorted
    /// or the index is out of range.
    /// </summary>
    /// <param name="index">The zero-based index of the pattern.</param>
    /// <returns>The <see cref="RecognizedPattern"/> at the given index, or <c>null</c>.</returns>
    public RecognizedPattern? Pattern(int index) => Patterns?.ElementAtOrDefault(index);

    /// <summary>
    /// Sorts the patterns by cost. Once sorted, the patterns list becomes accessible.
    /// </summary>
    /// <param name="sortDesc">
    /// When <c>true</c>, sorts in descending order; when <c>false</c>, in ascending order.
    /// </param>
    public void Sort(bool sortDesc)
    {
        if (IsSorted)
            return;

        _patterns.Sort(new RecognizedPatternComparer(sortDesc));
        IsSorted = true;
    }

    /// <summary>
    /// Renders a histogram of the recognition costs for all candidate patterns using SkiaSharp.
    /// Each bar represents a pattern, labeled with its character and scaled by cost.
    /// </summary>
    /// <returns>An <see cref="SKBitmap"/> containing the rendered histogram.</returns>
    public SKBitmap Render()
    {
        var width = 500;
        var height = 200;
        var histogram = new SKBitmap(width + 20, height + 20);
        using var canvas = new SKCanvas(histogram);

        // Background
        canvas.Clear(SKColors.LightGray);

        using var blackPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };
        using var font = new SKFont(SKTypeface.Default, 10);

        using var bluePaint = new SKPaint
        {
            Color = SKColors.Blue,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        var colWidth = width / _patterns.Count;

        // Draw Y-axis labels and tick marks
        for (var ay = 0; ay <= 100; ay += 10)
        {
            var y = 15 + (int)((100 - ay) / 100.0f * (height - 20));
            canvas.DrawText(ay.ToString(), 3, y + 11, SKTextAlign.Left, font, textPaint);
            canvas.DrawLine(25, y + 5, 35, y + 5, blackPaint);
        }

        // Draw Y-axis line
        canvas.DrawLine(35, 19, 35, height, blackPaint);

        // Draw bars
        for (var i = 0; i < _patterns.Count; i++)
        {
            var left = i * colWidth + 42;
            var top = height - (int)(_patterns[i].Cost * (height - 20));

            canvas.DrawRect(left, top, colWidth - 2, height - top, bluePaint);
            canvas.DrawText(_patterns[i].Char + " ", left + 2, top - 8, SKTextAlign.Left, font, textPaint);
        }

        return histogram;
    }
}
