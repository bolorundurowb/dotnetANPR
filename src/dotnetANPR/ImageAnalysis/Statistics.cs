using System;

namespace DotNetANPR.ImageAnalysis;

public class Statistics
{
    private readonly float _maximum;
    private readonly float _minimum;
    private readonly float _average;

    public Statistics(Photo photo)
    {
        float sum = 0;
        var width = photo.Width;
        var height = photo.Height;

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var pixelValue = photo.GetBrightness(x, y);
            _maximum = Math.Max(pixelValue, _maximum);
            _minimum = Math.Min(pixelValue, _minimum);
            sum += pixelValue;
        }

        _average = sum / (width * height);
    }

    public float ThresholdBrightness(float value, float coefficient)
    {
        if (value > _average)
            return coefficient + (1 - coefficient) * (value - _average) / (_maximum - _average);

        return (1 - coefficient) * (value - _minimum) / (_average - _minimum);
    }
}
