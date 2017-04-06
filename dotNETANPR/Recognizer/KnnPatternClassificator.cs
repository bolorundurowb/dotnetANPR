using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotNETANPR.ImageAnalysis;

namespace dotNETANPR.Recognizer
{
    public class KnnPatterClassificator : CharacterRecognizer, ICharacterRecognizer
    {
        private readonly List<List<double>> _learnLists;

        public KnnPatterClassificator()
        {
            string path = Intelligence.Intelligence.Configurator.GetPathProperty("char_learnAlphabetPath");
            string alphastring = "0123456789abcdefghijklmnopqrstuvwxyz";
            _learnLists = new List<List<double>>();
            for (int i = 0; i < alphastring.Length; i++) _learnLists.Add(null);


            foreach (string fileNameWithPath in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(fileNameWithPath);
                int alphaPosition = alphastring.IndexOf(fileName.ToLower()[0]);
                if (alphaPosition == -1) continue;

                Character imgChar = new Character(path + Path.DirectorySeparatorChar + fileName);
                imgChar.Normalize();
                _learnLists.Insert(alphaPosition, imgChar.ExtractFeatures());
            }

            for (int i = 0; i < alphastring.Length; i++)
                if (_learnLists.ElementAt(i) == null)
                    throw new IOException("Warning : alphabet in " + path + " is not complete");
        }
        
        public RecognizedChar Recognize(Character chr)
        {
            List<double> tested = chr.ExtractFeatures();
            int minx = 0;
            float minfx = float.PositiveInfinity;
            RecognizedChar recognized = new RecognizedChar();
            for (int x = 0; x < _learnLists.Count; x++)
            {
                float fx = SimplifiedEuclideanDistance(tested, _learnLists.ElementAt(x));
                recognized.AddPattern(new RecognizedChar.RecognizedPattern(Alphabet[x], fx));
            }
            recognized.Sort(0);
            return recognized;
        }

        public float Difference(List<double> vectorA, List<double> vectorB)
        {
            return vectorA.Select((t, x) => Math.Abs((float) (vectorA.ElementAt(x) - vectorB.ElementAt(x)))).Sum();
        }

        public float SimplifiedEuclideanDistance(List<double> vectorA, List<double> vectorB)
        {
            return vectorA.Select((t, x) => Math.Abs((float) (t - vectorB[x]))).Sum(partialDiff => partialDiff * partialDiff);
        }
    }
}
