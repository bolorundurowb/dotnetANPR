using System.Collections.Generic;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Replaces Graph.java.
/// Represents a 1D projection (histogram) of image properties.
/// </summary>
public abstract class ProjectionGraph(Photo photo)
{
    public class Peak(int left, int center, int right, float amplitude)
    {
        public int Left = left;
        public int Center = center;
        public int Right = right;
        public float Amplitude = amplitude;
        public int Width => Right - Left;
    }

    protected class ProbabilityDistributor(int size)
    {
        private readonly float[] _values = new float[size];
        public void Add(int pos, float val) { if (pos >= 0 && pos < _values.Length) _values[pos] += val; }
        public float GetValue(int pos) => (pos < 0 || pos >= _values.Length) ? 0 : _values[pos];
    }

    public float[] YValues { get; protected set; }
    public int Length => YValues.Length;
    public float MaxValue { get; protected set; } = 0;
    public List<Peak> Peaks { get; protected set; }

    protected ProbabilityDistributor _distributor;
    protected readonly Photo _sourcePhoto = photo;

    protected void Init(int length)
    {
        _distributor = new ProbabilityDistributor(length);
        YValues = new float[length];
        Peaks = new List<Peak>();
    }

    public void Normalize()
    {
        for (var i = 0; i < Length; i++)
        {
            YValues[i] = _distributor.GetValue(i);
            if (YValues[i] > MaxValue) MaxValue = YValues[i];
        }
        if (MaxValue == 0) return;
        for (var i = 0; i < Length; i++)
        {
            YValues[i] = YValues[i] / MaxValue;
        }
    }

    public abstract void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize);
}