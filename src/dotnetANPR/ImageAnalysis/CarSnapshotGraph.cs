// --- All derived graph classes follow this pattern ---

using System;
using System.Linq;

namespace DotNetANPR.ImageAnalysis;

public class CarSnapshotGraph : ProjectionGraph
{
    public CarSnapshotGraph(CarSnapshot carSnapshot, int graphRankFilter) : base(carSnapshot)
    {
        Init(carSnapshot.Height);
        var brightness = carSnapshot.GetBrightnessMatrix();
        var width = carSnapshot.Width;
        var rankFilter = new float[width];

        for (var y = 0; y < carSnapshot.Height; y++)
        {
            for (var x = 0; x < width; x++)
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
        var average = YValues.Average();
        var peakFoot = (float)((MaxValue - average) * peakFootConstant + average);
        var onPeak = YValues[0] > peakFoot;
        var peakLeft = onPeak ? 0 : -1;

        for (var x = 1; x < Length; x++)
        {
            if (onPeak)
            {
                if (YValues[x] < peakFoot) // End of peak
                {
                    var peakCenter = (x + peakLeft - 1) / 2;
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