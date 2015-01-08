using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.ImageAnalysis
{
    public class PlateHorizontalGraph : Graph
    {
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static double peakFootConstant =// 0.1;  /* CONSTANT*/
            configg.getDoubleProperty("platehorizontalgraph_peakfootconstant");
        private static int horizontalDetectionType =
                configg.getIntProperty("platehorizontalgraph_detectionType");

        Plate handle;

        public PlateHorizontalGraph(Plate handle)
        {
            this.handle = handle;
        }

        public float derivation(int index1, int index2)
        {
            return this.yValues.ElementAt(index1) - this.yValues.ElementAt(index2);
        }

        public List<Peak> findPeak(int count)
        {
            if (horizontalDetectionType == 1) return findPeak_edgedetection(count);
            return findPeak_derivate(count);
        }

        public List<Peak> findPeak_derivate(int count)
        {  // RIESENIE DERIVACIOU
            int a, b;
            float maxVal = this.getMaxValue();

            for (a = 2; -derivation(a, a + 4) < maxVal * 0.2 && a < this.yValues.Count - 2 - 2 - 4; a++) ;
            for (b = this.yValues.Count - 1 - 2; derivation(b - 4, b) < maxVal * 0.2 && b > a + 2; b--) ;

            List<Peak> outPeaks = new List<Peak>();

            outPeaks.Add(new Peak(a, b));
            base.peaks = outPeaks;
            return outPeaks;
        }

        public List<Peak> findPeak_edgedetection(int count)
        {
            float average = this.getAverageValue();
            int a, b;
            for (a = 0; this.yValues.ElementAt(a) < average; a++) ;
            for (b = this.yValues.Count - 1; this.yValues.ElementAt(b) < average; b--) ;

            List<Peak> outPeaks = new List<Peak>();
            a = Math.Max(a - 5, 0);
            b = Math.Min(b + 5, this.yValues.Count);

            outPeaks.Add(new Peak(a, b));
            base.peaks = outPeaks;
            return outPeaks;
        }
    }
}
