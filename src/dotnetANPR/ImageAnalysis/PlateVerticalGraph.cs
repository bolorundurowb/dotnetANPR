using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Replaces PlateVerticalGraph.java.
/// Finds horizontal peaks in a LicensePlate, corresponding to character rows.
/// </summary>
public class PlateVerticalGraph : ProjectionGraph
{
    public PlateVerticalGraph(LicensePlate plate) : base(plate)
    {
        Init(plate.Height);
        var brightness = plate.GetBrightnessMatrix();
        var width = plate.Width;
        var height = plate.Height;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Projecting the horizontal sum of dark pixels
                _distributor.Add(y, 1 - brightness[x, y]);
            }
        }
        Normalize();
    }

    public override void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
    {
        var peakFoot = (float)peakFootConstant;
        var onPeak = YValues[0] > peakFoot;
        var peakLeft = onPeak ? 0 : -1;

        for (var y = 1; y < Length; y++)
        {
            if (onPeak)
            {
                if (YValues[y] < peakFoot) // End of peak
                {
                    var peakCenter = (y + peakLeft - 1) / 2;
                    Peaks.Add(new Peak(peakLeft, peakCenter, y - 1, YValues[peakCenter]));
                    onPeak = false;
                    peakLeft = -1;
                }
            }
            else if (YValues[y] > peakFoot) // Start of peak
            {
                onPeak = true;
                peakLeft = y;
            }
        }

        // Handle peak at the end
        if (onPeak)
        {
            var peakCenter = (Length - 1 + peakLeft) / 2;
            Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
        }

        Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
    }
}