using System;

namespace DotNetANPR.ImageAnalysis
{
    public record HoughLine(int Radius, int Alpha, int Value, double AngleDegrees);

    /// <summary>
    /// Replaces HoughTransformation.java. Used for skew detection.
    /// </summary>
    public class HoughTransformation
    {
        private readonly int[,] _houghMap;
        private readonly int _width, _height;
        private readonly int _radiusMax;
        private readonly int _alphaMax = 180;
        private readonly double[] _cosCache;
        private readonly double[] _sinCache;

        public HoughTransformation(int width, int height)
        {
            _width = width;
            _height = height;
            _radiusMax = (int)Math.Ceiling(Math.Sqrt(width * width + height * height));
            _houghMap = new int[_radiusMax * 2, _alphaMax]; // *2 for negative radius

            // Pre-calculate sin/cos values
            _cosCache = new double[_alphaMax];
            _sinCache = new double[_alphaMax];
            for (int alpha = 0; alpha < _alphaMax; alpha++)
            {
                double rad = (alpha - 90) * Math.PI / 180.0;
                _cosCache[alpha] = Math.Cos(rad);
                _sinCache[alpha] = Math.Sin(rad);
            }
        }

        public void AddLine(int x, int y, float brightness)
        {
            if (brightness < 0.5) return; // Only process black pixels

            for (int alpha = 0; alpha < _alphaMax; alpha++)
            {
                int radius = (int)(x * _cosCache[alpha] + y * _sinCache[alpha]);
                int radiusOffset = radius + _radiusMax; // Offset to handle negative radius
                _houghMap[radiusOffset, alpha]++;
            }
        }

        public HoughLine GetBestLine()
        {
            int bestRadius = 0, bestAlpha = 0, bestValue = 0;
            for (int r = 0; r < _radiusMax * 2; r++)
            {
                for (int a = 0; a < _alphaMax; a++)
                {
                    if (_houghMap[r, a] > bestValue)
                    {
                        bestValue = _houghMap[r, a];
                        bestRadius = r - _radiusMax; // De-offset
                        bestAlpha = a - 90; // De-offset
                    }
                }
            }
            return new HoughLine(bestRadius, bestAlpha, bestValue, bestAlpha);
        }
    }
}