using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.ImageAnalysis
{
    public class PlateGraph : Graph
    {
        Plate handle;
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static double plategraph_rel_minpeaksize =
                configg.getDoubleProperty("plategraph_rel_minpeaksize");
        private static double peakFootConstant =
                configg.getDoubleProperty("plategraph_peakfootconstant");

        public PlateGraph(Plate handle)
        {
            this.handle = handle; // nesie odkaz na obrazok (Plate), ku ktoremu sa graf vztahuje
        }

        public class SpaceComparer : IComparer<Peak>
        {
            List<float> yValues = null;

            public SpaceComparer(List<float> yValues)
            {
                this.yValues = yValues;
            }

            private float getPeakValue(Object peak)
            {
                return ((Peak)peak).getCenter(); // left > right
                //return this.yValues.elementAt( ((Peak)peak).center()  );
            }

            public int Compare(Peak peak1, Peak peak2)
            {
                double comparison = this.getPeakValue(peak2) - this.getPeakValue(peak1);
                if (comparison < 0) return 1;
                if (comparison > 0) return -1;
                return 0;
            }
        }

        public List<Peak> findPeaks(int count)
        {
            List<Peak> spacesTemp = new List<Peak>();

            // uprava grafu pred segmentaciou : 
            // 1. zistime average value a maxval
            // 2. upravime minval ako 
            //  diffVal = average - (maxval - average) = 2average - maxval
            //  val -= diffVal

            float diffGVal = 2 * this.getAverageValue() - this.getMaxValue();

            List<float> yValuesNew = new List<float>();
            foreach (float f in this.yValues)
            {
                yValuesNew.Add(f - diffGVal);
            }
            this.yValues = yValuesNew;

            this.deActualizeFlags();
            // end

            for (int c = 0; c < count; c++)
            { // for count
                float maxValue = 0.0f;
                int maxIndex = 0;
                for (int i = 0; i < this.yValues.Count; i++)
                { // zlava doprava
                    if (allowedInterval(spacesTemp, i))
                    { // ak potencialny vrchol sa nachadza vo "volnom" intervale, ktory nespada pod ine vrcholy
                        if (this.yValues.ElementAt(i) >= maxValue)
                        {
                            maxValue = this.yValues.ElementAt(i);
                            maxIndex = i;
                        }
                    }
                } // end for int 0->max
                // nasli sme najvacsi peak
                // 0.75 mensie cislo znamena tendenciu znaky sekat, vacsie cislo zase tendenciu nespravne zdruzovat
                if (yValues.ElementAt(maxIndex) < plategraph_rel_minpeaksize * this.getMaxValue()) break;

                int leftIndex = indexOfLeftPeakRel(maxIndex, peakFootConstant); //urci sirku detekovanej medzery
                int rightIndex = indexOfRightPeakRel(maxIndex, peakFootConstant);

                spacesTemp.Add(new Peak(
                        Math.Max(0, leftIndex),
                        maxIndex,
                        Math.Min(this.yValues.Count - 1, rightIndex)
                        ));
            } // end for count

            // treba filtrovat kandidatov, ktory nezodpovedaju proporciam MEDZERY
            List<Peak> spaces = new List<Peak>();
            foreach (Peak p in spacesTemp)
            {
                if (p.getDiff() < 1 * this.handle.getHeight() // medzera nesmie byt siroka
                    ) spaces.Add(p);// znacka ok, bereme ju
                //else outPeaksFiltered.add(p);// znacka ok, bereme ju
            }

            // List<Peak> space OBSAHUJE MEDZERY, zoradime LEFT -> RIGHT
            spaces.Sort(new SpaceComparer(this.yValues));



            // outPeaksFiltered teraz obsahuje MEDZERY ... v nasledujucom kode
            // ich transformujeme na pismena
            List<Peak> chars = new List<Peak>();

            /*
             *      + +   +++           +++             +     
             *       + +++   +         +   +           +
             *                +       +     +         + 
             *                 +     +       +      ++
             *                  +   +         +   ++
             *                   +++           +++
             *                    |      |      1        |     2 ....
             *                    |  
             *                    +--> 1. local minimum 
             *
             */


            // zapocitame aj znak od medzery na lavo :
            if (spaces.Count != 0)
            {
                // detekujeme 1. lokalne minimum na grafe
                // 3 = leftmargin
                int minIndex = this.getMinValueIndex(0, spaces.ElementAt(0).getCenter());
                //System.out.println("minindex found at " + minIndex + " in interval 0 - " + outPeaksFiltered.elementAt(0).getCenter());
                // hladame index do lava od minindex
                int leftIndex = 0;
                //                for (int i=minIndex; i>=0; i--) {
                //                    leftIndex = i;
                //                    if (this.yValues.elementAt(i) > 
                //                        0.9 * this.yValues.elementAt(
                //                                outPeaksFiltered.elementAt(0).getCenter()
                //                                                    )
                //                       ) break;
                //                }

                Peak first = new Peak(leftIndex/*0*/, spaces.ElementAt(0).getCenter());
                if (first.getDiff() > 0) chars.Add(first);
            }

            for (int i = 0; i < spaces.Count - 1; i++)
            {
                int left = spaces.ElementAt(i).getCenter();
                int right = spaces.ElementAt(i + 1).getCenter();
                chars.Add(new Peak(left, right));
            }

            // znak ktory je napravo od poslednej medzery : 
            if (spaces.Count != 0)
            {
                Peak last = new Peak(
                    spaces.ElementAt(spaces.Count - 1).getCenter(),
                    this.yValues.Count - 1
                        );
                if (last.getDiff() > 0) chars.Add(last);
            }

            base.peaks = chars;
            return chars;

        }
        //        public int indexOfLeftPeak(int peak) {
        //            int index=peak;
        //            int counter = 0;
        //            for (int i=peak; i>=0; i--) {
        //                index = i;
        //                if (yValues.elementAt(index) < 0.7 * yValues.elementAt(peak) ) break;
        //            }
        //            return Math.max(0,index);
        //        }
        //        public int indexOfRightPeak(int peak) {
        //            int index=peak;
        //            int counter = 0;
        //            for (int i=peak; i<yValues.size(); i++) {
        //                index = i;
        //                if (yValues.elementAt(index) < 0.7 * yValues.elementAt(peak) ) break;
        //            }
        //            return Math.min(yValues.size(), index);
        //        }

        //        public float minValInInterval(float a, float b) {
        //            int ia = (int)(a*yValues.size());
        //            int ib = (int)(b*yValues.size());
        //            float min = float.POSITIVE_INFINITY;
        //            for (int i=ia; i<ib;i++) {
        //                min = Math.min(min, yValues.elementAt(i));
        //            }
        //            return min;
        //        }
        //        public float maxValInInterval(float a, float b) {
        //            int ia = (int)(a*yValues.size());
        //            int ib = (int)(b*yValues.size());
        //            float max = 0;
        //            for (int i=ia; i<ib;i++) {
        //                max = Math.max(max, yValues.elementAt(i));
        //            }
        //            return max;
        //        }

    }
}
