// --- All derived graph classes follow this pattern ---

using System;
using System.Linq;

namespace DotNetANPR.ImageAnalysis
{
    public class CarSnapshotGraph : ProjectionGraph
    {
        public CarSnapshotGraph(CarSnapshot carSnapshot, int graphRankFilter) : base(carSnapshot)
        {
            Init(carSnapshot.Height);
            float[,] brightness = carSnapshot.GetBrightnessMatrix();
            int width = carSnapshot.Width;
            var rankFilter = new float[width];

            for (int y = 0; y < carSnapshot.Height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rankFilter[x] = brightness[x, y];
                }
                Array.Sort(rankFilter);
                _distributor.Add(y, rankFilter[graphRankFilter]);
            }
            Normalize();
        }

        public override void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
        {
            float average = YValues.Average();
            float peakFoot = (float)((MaxValue - average) * peakFootConstant + average);
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
            Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
        }
    }
}