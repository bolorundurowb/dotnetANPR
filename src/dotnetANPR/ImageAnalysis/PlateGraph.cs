using System.Collections.Generic;
using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Replaces PlateGraph.java.
/// This graph is used to find both characters (peaks) and spaces (valleys).
/// </summary>
public class PlateGraph : ProjectionGraph
{
    public List<Peak> Spaces { get; private set; }

    public PlateGraph(LicensePlate plate) : base(plate)
    {
        Init(plate.Width);
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
        Spaces = [];
    }

    /// <summary>
    /// Finds peaks (characters) in the plate's horizontal projection.
    /// </summary>
    public override void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
    {
        var peakFoot = (float)peakFootConstant;
        var onPeak = YValues[0] > peakFoot;
        var peakLeft = onPeak ? 0 : -1;

        for (var x = 1; x < Length; x++)
        {
            if (onPeak)
            {
                if (YValues[x] < peakFoot) // End of peak
                {
                    var peakCenter = (x + peakLeft - 1) / 2;
                    if (YValues[peakCenter] > relativeMinPeakSize)
                    {
                        Peaks.Add(new Peak(peakLeft, peakCenter, x - 1, YValues[peakCenter]));
                    }
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

        // Handle peak at the end
        if (onPeak && YValues[(peakLeft + Length - 1) / 2] > relativeMinPeakSize)
        {
            var peakCenter = (Length - 1 + peakLeft) / 2;
            Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
        }
    }

    /// <summary>
    /// Finds valleys (spaces between characters).
    /// </summary>
    public void FindSpaces(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
    {
        var peakFoot = (float)peakFootConstant;
        // Note: onPeak here means "on a space" (low value)
        var onPeak = YValues[0] < peakFoot;
        var peakLeft = onPeak ? 0 : -1;

        for (var x = 1; x < Length; x++)
        {
            if (onPeak)
            {
                if (YValues[x] > peakFoot) // End of space
                {
                    var peakCenter = (x + peakLeft - 1) / 2;
                    if (YValues[peakCenter] < relativeMinPeakSize)
                    {
                        Spaces.Add(new Peak(peakLeft, peakCenter, x - 1, YValues[peakCenter]));
                    }
                    onPeak = false;
                    peakLeft = -1;
                }
            }
            else if (YValues[x] < peakFoot) // Start of space
            {
                onPeak = true;
                peakLeft = x;
            }
        }

        // Handle space at the end
        if (onPeak && YValues[(peakLeft + Length - 1) / 2] < relativeMinPeakSize)
        {
            var peakCenter = (Length - 1 + peakLeft) / 2;
            Spaces.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
        }

        Spaces = Spaces.OrderByDescending(s => s.Width).ToList();
    }
}