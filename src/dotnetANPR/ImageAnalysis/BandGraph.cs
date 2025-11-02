using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis
{
    /// <summary>
    /// Replaces BandGraph.java.
    /// Finds vertical peaks in a LicensePlateBand, corresponding to potential plates.
    /// </summary>
    public class BandGraph : ProjectionGraph
    {
        public BandGraph(LicensePlateBand band) : base(band)
        {
            Init(band.Width);
            float[,] brightness = band.GetBrightnessMatrix();
            int width = band.Width;
            int height = band.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Projecting the vertical sum of dark pixels (1 - brightness)
                    _distributor.Add(x, 1 - brightness[x, y]);
                }
            }
            Normalize();
        }

        public override void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
        {
            float average = YValues.Average();
            float peakFoot = (float)((MaxValue - average)* peakFootConstant + average);
            bool onPeak = YValues[0] > peakFoot;
            int peakLeft = onPeak ? 0 : -1;

            for (int x = 1; x < Length; x++)
            {
                if (onPeak)
                {
                    if (YValues[x] < peakFoot) // End of peak
                    {
                        int peakCenter = (x + peakLeft - 1) / 2;
                        Peaks.Add(new Peak(peakLeft, peakCenter, x - 1, YValues[peakCenter]));
                        onPeak = false;
                        peakLeft = -1;
                    }
                }
                else if (YValues[x] > peakFoot) // Start of peak
                {
                    onPeak = true;
                    peakLeft = x;
                }
            }
            
            // Handle peak at the end of the graph
            if (onPeak)
            {
                int peakCenter = (Length - 1 + peakLeft) / 2;
                Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
            }

            Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
        }
    }
}