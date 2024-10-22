﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

    public Bitmap Render()
    {
        var width = 500;
        var height = 200;
        var histogram = new Bitmap(width + 20, height + 20);
        using var graphic = Graphics.FromImage(histogram);
        graphic.Clear(Color.LightGray);
        var backRect = new Rectangle(0, 0, width + 20, height + 20);
        graphic.FillRectangle(Brushes.LightGray, backRect);
        graphic.DrawRectangle(Pens.Black, backRect);

        graphic.DrawString("0", new Font("Arial", 8), Brushes.Black, new PointF(3, height - 5));

        var colWidth = width / _patterns.Count;
        int left, top;

        for (var ay = 0; ay <= 100; ay += 10)
        {
            var y = 15 + (int)((100 - ay) / 100.0f * (height - 20));
            graphic.DrawString(ay.ToString(), new Font("Arial", 8), Brushes.Black, new PointF(3, y + 11));
            graphic.DrawLine(Pens.Black, 25, y + 5, 35, y + 5);
        }

        graphic.DrawLine(Pens.Black, 35, 19, 35, height);
        graphic.DrawString("100", new Font("Arial", 8), Brushes.Black,
            new PointF(3, 15 + (int)((100 - 100) / 100.0f * (height - 20)) + 11));

        graphic.DrawLine(Pens.Black, 35, 19, 35, height);

        graphic.DrawLine(Pens.Black, 35, 19, 35, height);

        for (var i = 0; i < _patterns.Count; i++)
        {
            left = i * colWidth + 42;
            top = height - (int)(_patterns[i].Cost * (height - 20));
            graphic.DrawRectangle(Pens.Blue, left, top, colWidth - 2, height - top);
            graphic.DrawString(_patterns[i].Char + " ", new Font("Arial", 8), Brushes.Black,
                new PointF(left + 2, top - 8));
        }

        return histogram;
    }
}
