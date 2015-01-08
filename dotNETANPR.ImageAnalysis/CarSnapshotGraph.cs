using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace dotNETANPR.ImageAnalysis
{
    public class CarSnapshotGraph : Graph
    {
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static double peakFootConstant = 
            configg.getDoubleProperty("carsnapshotgraph_peakfootconstant"); //0.55
    private static double peakDiffMultiplicationConstant = 
            configg.getDoubleProperty("carsnapshotgraph_peakDiffMultiplicationConstant");//0.1
    
    CarSnapshot handle;
    
    public CarSnapshotGraph(CarSnapshot handle) 
    {
        this.handle = handle;
    }
    
    public class PeakComparer : IComparer <Peak>
    {
        List<float> yValues = null;
        
        public PeakComparer(List<float> yValues)
        {
            this.yValues = yValues;
        }
        
        private float getPeakValue(Object peak)
        {
            return this.yValues.ElementAt( ((Peak)peak).getCenter()  ); // podla intenzity
            //return ((Peak)peak).getDiff();
        }
        
        public int compare(Object peak1, Object peak2) { // Peak
            double comparison = this.getPeakValue(peak2) - this.getPeakValue(peak1);
            if (comparison < 0) return -1;
            if (comparison > 0) return 1;
            return 0;
        }
    }
    
    public List<Peak> findPeaks(int count) 
    {
        
        List<Peak> outPeaks = new List<Peak>();
        
        for (int c=0; c<count; c++) 
        { // for count
            float maxValue = 0.0f;
            int maxIndex = 0;
            for (int i=0; i<this.yValues.Count; i++)
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
            int leftIndex = indexOfLeftPeakRel(maxIndex,peakFootConstant);
            int rightIndex = indexOfRightPeakRel(maxIndex,peakFootConstant);
            int diff = rightIndex - leftIndex;
            leftIndex -= (int)peakDiffMultiplicationConstant * diff;   /*CONSTANT*/
            rightIndex+= (int)peakDiffMultiplicationConstant * diff;   /*CONSTANT*/

                outPeaks.Add(new Peak(
                    Math.Max(0,leftIndex),
                    maxIndex,
                    Math.Min(this.yValues.Count-1,rightIndex)
                    ));
        } // end for count
        
        outPeaks.Sort(new PeakComparer(this.yValues));
        
        base.peaks = outPeaks; 
        return outPeaks;
    }
//    public int indexOfLeftPeak(int peak, double peakFootConstant) {
//        int index=peak;
//        for (int i=peak; i>=0; i--) {
//            index = i;
//            if (yValues.elementAt(index) < peakFootConstant*yValues.elementAt(peak) ) break;
//        }
//        return Math.max(0,index);
//    }
//    public int indexOfRightPeak(int peak, double peakFootConstant) {
//        int index=peak;
//        for (int i=peak; i<yValues.size(); i++) {
//            index = i;
//            if (yValues.elementAt(index) < peakFootConstant*yValues.elementAt(peak) ) break;
//        }
//        return Math.min(yValues.size(), index);
//    }
    
    }
}
