using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.ImageAnalysis
{
    class BandGraph : Graph
    {
        Band handle;
        static Configurator.Configurator config = new Configurator.Configurator();
        private static double peakFootConstant = config.GetDoubleProperty("bandgraph_peakfootconstant"); //0.75
        private static double peakDiffMultiplicationConstant = config.GetDoubleProperty("bandgraph_peakDiffMultiplicationConstant");  // 0.2


        public BandGraph(Band handle)
        {
            this.handle = handle;
        }

        public class PeakComparer : IComparer<Peak>
        {
            List<float> yValues = null;

            public PeakComparer(List<float> yValues)
            {
                this.yValues = yValues;
            }

            private float getPeakValue(Peak peak)
            {
                return this.yValues.ElementAt(peak.GetCenter());
            }

            public int Compare(Peak peak1, Peak peak2)
            {
                double comparison = getPeakValue(peak2) - getPeakValue(peak1);
                if (comparison < 0) return -1;
                if (comparison > 0) return 1;
                return 0;
            }
        }

        public List<Peak> findPeaks(int count)
        {
            List<Graph.Peak> outPeaks = new List<Peak>();
            for (int c = 0; c < count; c++)
            {
                float maxValue = 0.0f;
                int maxIndex = 0;
                for (int i = 0; i < this.yValues.Count; i++)
                {
                    if (allowedInterval(outPeaks, i))
                    {
                        if (this.yValues.ElementAt(i) >= maxValue)
                        {
                            maxValue = this.yValues.ElementAt(i);
                            maxIndex = i;
                        }
                    }
                }
                int leftIndex = indexOfLeftPeakRel(maxIndex, peakFootConstant);
                int rightIndex = indexOfRightPeakRel(maxIndex, peakFootConstant);
                int diff = rightIndex - leftIndex;
                leftIndex -= (int)peakDiffMultiplicationConstant * diff;
                rightIndex += (int)peakDiffMultiplicationConstant * diff;


                outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex,
                        Math.Min(this.yValues.Count - 1, rightIndex)
                        ));
            }

            List<Peak> outPeaksFiltered = new List<Peak>();
            foreach (Peak p in outPeaks)
            {
                if (p.GetDiff() > 2 * handle.GetHeight() &&
                    p.GetDiff() < 15 * handle.GetHeight())
                    outPeaksFiltered.Add(p);
            }

            outPeaksFiltered.Sort(new PeakComparer(this.yValues));
            base.peaks = outPeaksFiltered;
            return outPeaksFiltered;

        }
        public int indexOfLeftPeakAbs(int peak, double peakFootConstantAbs)
        {
            int index = peak;
            int counter = 0;
            for (int i = peak; i >= 0; i--)
            {
                index = i;
                if (yValues.ElementAt(index) < peakFootConstantAbs) break;
            }
            return Math.Max(0, index);
        }
        public int indexOfRightPeakAbs(int peak, double peakFootConstantAbs)
        {
            int index = peak;
            int counter = 0;
            for (int i = peak; i < yValues.Count; i++)
            {
                index = i;
                if (yValues.ElementAt(index) < peakFootConstantAbs) break;
            }
            return Math.Min(yValues.Count, index);
        }
    }
}
