using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Replaces BandGraph.java.
/// Finds vertical peaks in a LicensePlateBand, corresponding to potential plates.
/// </summary>
public class BandGraph : ProjectionGraph
{
    public BandGraph(LicensePlateBand band) : base(band)
    {
        Init(band.Width);
        var brightness = band.GetBrightnessMatrix();
        var width = band.Width;
        var height = band.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                // Projecting the vertical sum of dark pixels (1 - brightness)
                _distributor.Add(x, 1 - brightness[x, y]);
            }
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

        // Handle peak at the end of the graph
        if (onPeak)
        {
            var peakCenter = (Length - 1 + peakLeft) / 2;
            Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
        }

        Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
    }
}