using System;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Computes brightness statistics (minimum, maximum, and average) for a <see cref="Photo"/>
/// and provides a threshold brightness normalization function.
/// </summary>
public class Statistics
{
    private readonly float _maximum;
    private readonly float _minimum;
    private readonly float _average;

    /// <summary>
    /// Initializes a new instance of the <see cref="Statistics"/> class by scanning all pixels
    /// of the given photo to compute brightness statistics.
    /// </summary>
    /// <param name="photo">The photo to analyze.</param>
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

    /// <summary>
    /// Computes a threshold-adjusted brightness value. Values above the average are mapped
    /// to the upper range, values below are mapped to the lower range, scaled by the coefficient.
    /// </summary>
    /// <param name="value">The original brightness value (0.0 to 1.0).</param>
    /// <param name="coefficient">The threshold coefficient controlling contrast.</param>
    /// <returns>The threshold-adjusted brightness value, or 0 if the result is NaN.</returns>
    public float ThresholdBrightness(float value, float coefficient)
    {
        var threshold = (value > _average)
            ? coefficient + (1 - coefficient) * (value - _average) / (_maximum - _average)
            : (1 - coefficient) * (value - _minimum) / (_average - _minimum);

        return float.IsNaN(threshold) ? 0 : threshold;
    }
}
