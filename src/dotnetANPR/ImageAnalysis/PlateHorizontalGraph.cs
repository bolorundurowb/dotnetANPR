using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Replaces PlateHorizontalGraph.java.
/// An alternative horizontal projection graph for plates, with two detection types.
/// </summary>
public class PlateHorizontalGraph : ProjectionGraph
{
    private readonly int _detectionType;

    public PlateHorizontalGraph(LicensePlate plate, int detectionType) : base(plate)
    {
        Init(plate.Width);
        _detectionType = detectionType;
        var brightness = plate.GetBrightnessMatrix();
        var width = plate.Width;
        var height = plate.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                _distributor.Add(x, 1 - brightness[x, y]);
            }
        }
        Normalize();
    }

    public override void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
    {
        if (_detectionType == 0)
            FindPeaksStandard((float)peakFootConstant);
        else
            FindPeaksAlternative((float)peakFootConstant);
    }

    // Standard peak finding
    private void FindPeaksStandard(float peakFoot)
    {
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

        if (onPeak) // Handle peak at the end
        {
            var peakCenter = (Length - 1 + peakLeft) / 2;
            Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
        }

        Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
    }

    // Alternative peak finding (less strict)
    private void FindPeaksAlternative(float peakFoot)
    {
        var onPeak = YValues[0] > peakFoot;
        var peakLeft = onPeak ? 0 : -1;

        for (var x = 1; x < Length; x++)
        {
            if (YValues[x] > peakFoot)
            {
                if (!onPeak)
                {
                    peakLeft = x;
                    onPeak = true;
                }
            }
            else if (onPeak)
            {
                var peakCenter = (x + peakLeft - 1) / 2;
                Peaks.Add(new Peak(peakLeft, peakCenter, x - 1, YValues[peakCenter]));
                onPeak = false;
                peakLeft = -1;
            }
        }

        if (onPeak) // Handle peak at the end
        {
            var peakCenter = (Length - 1 + peakLeft) / 2;
            Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
        }

        Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
    }
}