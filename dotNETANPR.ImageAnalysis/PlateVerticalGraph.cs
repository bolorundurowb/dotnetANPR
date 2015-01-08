using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.ImageAnalysis
{
    public class PlateVerticalGraph : Graph
    {
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static double peakFootConstant =// 0.42;  /* CONSTANT*/
            configg.getDoubleProperty("plateverticalgraph_peakfootconstant");

        Plate handle;

        public PlateVerticalGraph(Plate handle)
        {
            this.handle = handle;
        }

        public class PeakComparer : IComparer<Peak>
        {
            PlateVerticalGraph graphHandle = null;

            public PeakComparer(PlateVerticalGraph graph)
            {
                this.graphHandle = graph;
            }

            private float getPeakValue(Object peak)
            {
                // heuristika : aky vysoky (siroky na gragfe) je kandidat na pismeno
                // preferuju sa vyssie
                //return ((Peak)peak).getDiff();

                // vyska peaku
                return this.graphHandle.yValues.ElementAt(((Peak)peak).getCenter());

                // heuristika : 
                // ako daleko od stredu je kandidat
                //            int peakCenter = (  ((Peak)peak).getRight() + ((Peak)peak).getLeft()  )/2;
                //            return Math.abs(peakCenter - this.graphHandle.yValues.size()/2);
            }

            public int Compare(Peak peak1, Peak peak2)
            { // Peak
                double comparison = this.getPeakValue(peak2) - this.getPeakValue(peak1);
                if (comparison < 0) return -1;
                if (comparison > 0) return 1;
                return 0;
            }
        }

        public List<Peak> findPeak(int count)
        {

            // znizime peak
            for (int i = 0; i < this.yValues.Count; i++)
                this.yValues.Insert(i, this.yValues.ElementAt(i) - this.getMinValue());

            List<Peak> outPeaks = new List<Peak>();

            for (int c = 0; c < count; c++)
            { // for count
                float maxValue = 0.0f;
                int maxIndex = 0;
                for (int i = 0; i < this.yValues.Count; i++)
                { // zlava doprava
                    if (allowedInterval(outPeaks, i))
                    { // ak potencialny vrchol sa nachadza vo "volnom" intervale, ktory nespada pod ine vrcholy
                        if (this.yValues.ElementAt(i) >= maxValue)
                        {
                            maxValue = this.yValues.ElementAt(i);
                            maxIndex = i;
                        }
                    }
                } // end for int 0->max
                // nasli sme najvacsi peak

                if (yValues.ElementAt(maxIndex) < 0.05 * base.getMaxValue()) break;//0.4

                int leftIndex = indexOfLeftPeakRel(maxIndex, peakFootConstant);
                int rightIndex = indexOfRightPeakRel(maxIndex, peakFootConstant);

                outPeaks.Add(new Peak(
                        Math.Max(0, leftIndex),
                        maxIndex,
                        Math.Min(this.yValues.Count - 1, rightIndex)
                        ));
            }

            outPeaks.Sort(new PeakComparer(this));
            base.peaks = outPeaks;
            return outPeaks;
        }




    }
}
