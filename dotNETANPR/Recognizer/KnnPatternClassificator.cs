using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotNETANPR.ImageAnalysis;

namespace dotNETANPR.Recognizer
{
    public class KnnPatterClassificator : CharacterRecognizer
    {
        private readonly List<List<double>> learnLists;

        public KnnPatterClassificator()
        {
            string path = Intelligence.Intelligence.Configurator.GetPathProperty("char_learnAlphabetPath");
            string alphastring = "0123456789abcdefghijklmnopqrstuvwxyz";
            learnLists = new List<List<double>>();
            for (int i = 0; i < alphastring.Length; i++) learnLists.Add(null);


            foreach (string fileName in Directory.GetFiles(path))
            {
                int alphaPosition = alphastring.IndexOf(fileName.ToLower()[0]);
                if (alphaPosition == -1) continue;

                Character imgChar = new Character(path + Path.DirectorySeparatorChar + fileName);
                imgChar.Normalize();
                learnLists.Insert(alphaPosition, imgChar.ExtractFeatures());
            }

            for (int i = 0; i < alphastring.Length; i++)
                if (learnLists.ElementAt(i) == null)
                    throw new IOException("Warning : alphabet in " + path + " is not complete");
        }
        
        public RecognizedChar Recognize(Character chr)
        {
            List<double> tested = chr.ExtractFeatures();
            int minx = 0;
            float minfx = float.PositiveInfinity;
            RecognizedChar recognized = new RecognizedChar();
            for (int x = 0; x < learnLists.Count; x++)
            {
                float fx = SimplifiedEuclideanDistance(tested, learnLists.ElementAt(x));
                recognized.AddPattern(new RecognizedChar.RecognizedPattern(alphabet[x], fx));
            }
            recognized.Sort(0);
            return recognized;
        }

        public float Difference(List<double> vectorA, List<double> vectorB)
        {
            float diff = 0;
            for (int x = 0; x < vectorA.Count; x++)
            {
                diff += Math.Abs((float)(vectorA.ElementAt(x) - vectorB.ElementAt(x)));
            }
            return diff;
        }

        public float SimplifiedEuclideanDistance(List<double> vectorA, List<double> vectorB)
        {
            float diff = 0;
            for (int x = 0; x < vectorA.Count; x++)
            {
                var partialDiff = Math.Abs((float)(vectorA[x] - vectorB[x]));
                diff += partialDiff * partialDiff;
            }
            return diff;
        }
    }
}
