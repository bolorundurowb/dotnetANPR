using System.Collections.Generic;
using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis
{
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
            float[,] brightness = plate.GetBrightnessMatrix();
            int width = plate.Width;
            int height = plate.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _distributor.Add(x, 1 - brightness[x, y]);
                }
            }
            Normalize();
            Spaces = new List<Peak>();
        }

        /// <summary>
        /// Finds peaks (characters) in the plate's horizontal projection.
        /// </summary>
        public override void FindPeaks(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
        {
            float peakFoot = (float)peakFootConstant;
            bool onPeak = YValues[0] > peakFoot;
            int peakLeft = onPeak ? 0 : -1;

            for (int x = 1; x < Length; x++)
            {
                if (onPeak)
                {
                    if (YValues[x] < peakFoot) // End of peak
                    {
                        int peakCenter = (x + peakLeft - 1) / 2;
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
                 int peakCenter = (Length - 1 + peakLeft) / 2;
                 Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
            }
        }

        /// <summary>
        /// Finds valleys (spaces between characters).
        /// </summary>
        public void FindSpaces(double peakFootConstant, double peakDiffMultiplicationConstant, double relativeMinPeakSize)
        {
            float peakFoot = (float)peakFootConstant;
            // Note: onPeak here means "on a space" (low value)
            bool onPeak = YValues[0] < peakFoot;
            int peakLeft = onPeak ? 0 : -1;

            for (int x = 1; x < Length; x++)
            {
                if (onPeak)
                {
                    if (YValues[x] > peakFoot) // End of space
                    {
                        int peakCenter = (x + peakLeft - 1) / 2;
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
                 int peakCenter = (Length - 1 + peakLeft) / 2;
                 Spaces.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
            }
            
            Spaces = Spaces.OrderByDescending(s => s.Width).ToList();
        }
    }
}