using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace dotNETANPR.Recognizer
{
    class KnnPatternClassificator : CharacterRecognizer
    {
        List<List<Double>> learnLists;

        public KnnPatternClassificator()
        {
            Configurator.Configurator config = new Configurator.Configurator();
            string path = config.GetPathProperty("char_learnAlphabetPath");
            string alphastring = "0123456789abcdefghijklmnopqrstuvwxyz";

            this.learnLists = new List<List<Double>>();
            for (int i = 0; i < alphastring.Length; i++) this.learnLists.Add(null);

            foreach (string fileName in Directory.GetFiles(path))
            {
                int alphaPosition = alphastring.IndexOf(fileName.ToLower()[0]);
                if (alphaPosition == -1) continue; 
                ImageAnalysis.Char imgChar = new ImageAnalysis.Char(path + Path.DirectorySeparatorChar + fileName);
                imgChar.Normalize();
                this.learnLists.Insert(alphaPosition, imgChar.ExtractFeatures());
            }

            for (int i = 0; i < alphastring.Length; i++)
                if (this.learnLists.ElementAt(i) == null) throw new IOException("Warning : alphabet in " + path + " is not complete");

        }

        public RecognizedChar Recognize(ImageAnalysis.Char chr)
        {
            List<Double> tested = chr.ExtractFeatures();
            int minx = 0;
            float minfx = float.PositiveInfinity;

            RecognizedChar recognized = new RecognizedChar();

            for (int x = 0; x < this.learnLists.Count; x++)
            {
                float fx = this.simplifiedEuclideanDistance(tested, this.learnLists.ElementAt(x));

                recognized.addPattern(new RecognizedChar.RecognizedPattern(Alphabet[x], fx));
            }
            recognized.sort(0);
            return recognized;
        }

        public float Difference(List<Double> vectorA, List<Double> vectorB)
        {
            float diff = 0;
            for (int x = 0; x < vectorA.Count; x++)
            {
                diff += Math.Abs((float)(vectorA.ElementAt(x) - vectorB.ElementAt(x)));
            }
            return diff;
        }

        public float SimplifiedEuclideanDistance(List<Double> vectorA, List<Double> vectorB)
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
