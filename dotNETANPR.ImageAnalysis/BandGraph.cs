using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotNETANPR.Configurator;
using dotNETANPR.Recognizer;

namespace dotNETANPR.ImageAnalysis
{
    public class BandGraph : Graph
    {
        Band handle;
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static double peakFootConstant =   configg.getDoubleProperty("bandgraph_peakfootconstant"); //0.75
        private static double peakDiffMultiplicationConstant = configg.getDoubleProperty("bandgraph_peakDiffMultiplicationConstant");  // 0.2


        public BandGraph(Band handle)
        {
            this.handle = handle; // nesie odkaz na obrazok (band), ku ktoremu sa graf vztahuje
        }

        public class PeakComparer : IComparer<Peak>
        {
            List<float> yValues = null;

            public PeakComparer(List<float> yValues)
            {
                this.yValues = yValues;
            }

            private float getPeakValue(Object peak)
            {
                //return ((Peak)peak).center(); // left > right

                return this.yValues.ElementAt(((Peak)peak).getCenter()); // velkost peaku
            }

            public int CompareTo(Peak peak1, Peak peak2)
            {
                double comparison = this.getPeakValue(peak2) - this.getPeakValue(peak1);
                if (comparison < 0) return -1;
                if (comparison > 0) return 1;
                return 0;
            }
        }

        public List<Peak> findPeaks(int count) 
        {
            List<Graph.Peak> outPeaks = new List<Peak>();
            
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
                
                // nasli sme najvacsi peak // urobime 1. vysek
                int leftIndex = indexOfLeftPeakRel(maxIndex,peakFootConstant);
                int rightIndex = indexOfRightPeakRel(maxIndex,peakFootConstant);
                int diff = rightIndex - leftIndex;
                leftIndex -= (int)peakDiffMultiplicationConstant * diff;   /*CONSTANT*/
                rightIndex+= (int)peakDiffMultiplicationConstant * diff;   /*CONSTANT*/
               
                
                
                outPeaks.Add(new Peak(                        Math.Max(0,leftIndex),               maxIndex,
                        Math.Min(this.yValues.Count-1,rightIndex)
                        ));
            } // end for count
            

            
            // treba filtrovat kandidatov, ktory nezodpovedaju proporciam znacky
            List<Peak> outPeaksFiltered = new List<Peak>();
            foreach (Peak p in outPeaks)
            {
                if (p.getDiff() > 2 * this.handle.getHeight() && // ak nieje znacka prilis uzka
                    p.getDiff() < 15 * this.handle.getHeight() // alebo nie je prilis siroka
                    ) outPeaksFiltered.Add(p);// znacka ok, bereme ju
               // else outPeaksFiltered.add(p);// znacka ok, bereme ju
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
        /* TODO - END */
    }
}
