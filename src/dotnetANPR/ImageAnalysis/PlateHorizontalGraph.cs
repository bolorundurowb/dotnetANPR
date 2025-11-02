using System.Linq;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.ImageAnalysis
{
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
            
            if (onPeak) // Handle peak at the end
            {
                int peakCenter = (Length - 1 + peakLeft) / 2;
                Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
            }
            
            Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
        }

        // Alternative peak finding (less strict)
        private void FindPeaksAlternative(float peakFoot)
        {
            bool onPeak = YValues[0] > peakFoot;
            int peakLeft = onPeak ? 0 : -1;

            for (int x = 1; x < Length; x++)
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
                    int peakCenter = (x + peakLeft - 1) / 2;
                    Peaks.Add(new Peak(peakLeft, peakCenter, x - 1, YValues[peakCenter]));
                    onPeak = false;
                    peakLeft = -1;
                }
            }
            
            if (onPeak) // Handle peak at the end
            {
                int peakCenter = (Length - 1 + peakLeft) / 2;
                Peaks.Add(new Peak(peakLeft, peakCenter, Length - 1, YValues[peakCenter]));
            }

            Peaks = Peaks.OrderByDescending(p => p.Amplitude).ToList();
        }
    }
}