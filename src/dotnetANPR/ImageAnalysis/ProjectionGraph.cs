using System.Collections.Generic;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis
{
    /// <summary>
    /// Replaces Graph.java.
    /// Represents a 1D projection (histogram) of image properties.
    /// </summary>
    public abstract class ProjectionGraph
    {
        public class Peak
        {
            public int Left;
            public int Center;
            public int Right;
            public float Amplitude;
            public int Width => Right - Left;

            public Peak(int left, int center, int right, float amplitude)
            {
                Left = left; Center = center; Right = right; Amplitude = amplitude;
            }
        }

        protected class ProbabilityDistributor
        {
            private readonly float[] _values;
            public ProbabilityDistributor(int size) { _values = new float[size]; }
            public void Add(int pos, float val) { if (pos >= 0 && pos < _values.Length) _values[pos] += val; }
            public float GetValue(int pos) => (pos < 0 || pos >= _values.Length) ? 0 : _values[pos];
        }

        public float[] YValues { get; protected set; }
        public int Length => YValues.Length;
        public float MaxValue { get; protected set; } = 0;
        public List<Peak> Peaks { get; protected set; }

        protected ProbabilityDistributor _distributor;
        protected readonly Photo _sourcePhoto;

        protected ProjectionGraph(Photo photo)
        {
            _sourcePhoto = photo;
        }

        protected void Init(int length)
        {
            _distributor = new ProbabilityDistributor(length);
            YValues = new float[length];
            Peaks = new List<Peak>();
        }

        public void Normalize()
        {
            for (int i = 0; i < Length; i++)
            {
                YValues[i] = _distributor.GetValue(i);
                if (YValues[i] > MaxValue) MaxValue = YValues[i];
            }
            if (MaxValue == 0) return;
            for (int i = 0; i < Length; i++)
            {
                YValues[i] = YValues[i] / MaxValue;
            }
        }

        public abstract void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize);
    }
}