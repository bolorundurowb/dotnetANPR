using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.ImageAnalysis
{
    class CarSnapshotGraph : Graph
    {
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static double peakFootConstant =
            configg.GetDoubleProperty("carsnapshotgraph_peakfootconstant"); //0.55
        private static double peakDiffMultiplicationConstant =
                configg.GetDoubleProperty("carsnapshotgraph_peakDiffMultiplicationConstant");//0.1

        CarSnapshot handle;

        public CarSnapshotGraph(CarSnapshot handle)
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

            private float GetPeakValue(Peak peak)
            {
                return yValues.ElementAt(peak.GetCenter()); 
                                                                   
            }

            public int Compare(Peak peak1, Peak peak2)
            { 
                double comparison = this.GetPeakValue(peak2) - this.GetPeakValue(peak1);
                if (comparison < 0) return -1;
                if (comparison > 0) return 1;
                return 0;
            }
        }

        public List<Peak> FindPeaks(int count)
        {
            List<Peak> outPeaks = new List<Peak>();

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

                outPeaks.Add(new Peak(Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(this.yValues.Count - 1, rightIndex)
                    ));
            } 

            outPeaks.Sort(new PeakComparer(this.yValues));

            base.peaks = outPeaks;
            return outPeaks;
        }
    }
}
