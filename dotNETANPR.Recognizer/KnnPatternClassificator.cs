using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotNETANPR.Configurator;
using dotNETANPR.ImageAnalysis;
using System.IO;

namespace dotNETANPR.Recognizer
{
    public class KnnPatternClassificator : CharacterRecognizer
    {
        List<List<Double>> learnLists;

        public KnnPatternClassificator()
        {
            Configurator.Configurator config = new Configurator.Configurator();
            string path = config.getPathProperty("char_learnAlphabetPath");
            string alphastring = "0123456789abcdefghijklmnopqrstuvwxyz";

            // inicializacia vektora na pozadovanu velkost (nulovanie poloziek)
            this.learnLists = new List<List<Double>>();
            for (int i = 0; i < alphastring.Length; i++) this.learnLists.Add(null);


            foreach (string fileName in Directory.GetFiles(path))
            {
                int alphaPosition = alphastring.IndexOf(fileName.ToLower()[0]);
                if (alphaPosition == -1) continue; // je to nezname meno suboru, skip

                ImageAnalysis.Char imgChar = new ImageAnalysis.Char(path + Path.DirectorySeparatorChar + fileName);
                imgChar.normalize();
                // zapis na danu poziciu vo vektore
                this.learnLists.Insert(alphaPosition, imgChar.extractFeatures());
            }

            // kontrola poloziek vektora
            for (int i = 0; i < alphastring.Length; i++)
                if (this.learnLists.ElementAt(i) == null) throw new IOException("Warning : alphabet in " + path + " is not complete");

        }

        public CharacterRecognizer.RecognizedChar recognize(ImageAnalysis.Char chr)
    {
        List<Double> tested = chr.extractFeatures();
        int minx = 0;
        float minfx = float.PositiveInfinity;
        
        CharacterRecognizer.RecognizedChar recognized = new CharacterRecognizer.RecognizedChar();
        
        for (int x = 0; x < this.learnLists.Count; x++) 
        {
            // pre lepsie fungovanie bol pouhy rozdiel vektorov nahradeny euklidovskou vzdialenostou
            float fx = this.simplifiedEuclideanDistance(tested, this.learnLists.ElementAt(x));

            recognized.addPattern(new CharacterRecognizer.RecognizedChar.RecognizedPattern(alphabet[x], fx));
            
            //if (fx < minfx) {
            //    minfx = fx;
            //    minx = x;
            //}
        }
//        return new RecognizedChar(this.alphabet[minx], minfx);
        recognized.sort(0);
        return recognized;
    }

        public float difference(List<Double> vectorA, List<Double> vectorB)
        {
            float diff = 0;
            for (int x = 0; x < vectorA.Count; x++)
            {
                diff += Math.Abs((float)(vectorA.ElementAt(x) - vectorB.ElementAt(x)));
            }
            return diff;
        }

        public float simplifiedEuclideanDistance(List<Double> vectorA, List<Double> vectorB)
        {
            float diff = 0;
            float partialDiff;
            for (int x = 0; x < vectorA.Count; x++)
            {
                partialDiff = (float)Math.Abs((float)(vectorA.ElementAt(x) - vectorB.ElementAt(x)));
                diff += partialDiff * partialDiff;
            }
            return diff;
        }
    }
}
