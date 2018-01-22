using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotnetANPR.ImageAnalysis;

namespace dotnetANPR.Recognizer
{
    public class KnnPatternClassificator : CharacterRecognizer, ICharacterRecognizer
    {
        private readonly List<List<double>> _learnLists;

        public KnnPatternClassificator()
        {
            var path = Intelligence.Intelligence.Configurator.GetPathProperty("char_learnAlphabetPath");
            var alphastring = "0123456789abcdefghijklmnopqrstuvwxyz";
            _learnLists = new List<List<double>>();
            for (var i = 0; i < alphastring.Length; i++) _learnLists.Add(null);


            foreach (var fileNameWithPath in Directory.GetFiles(path))
            {
                var fileName = Path.GetFileName(fileNameWithPath);
                var alphaPosition = alphastring.IndexOf(fileName.ToLower()[0]);
                if (alphaPosition == -1) continue;

                var imgChar = new Character(path + Path.DirectorySeparatorChar + fileName);
                imgChar.Normalize();
                _learnLists.Insert(alphaPosition, imgChar.ExtractFeatures());
            }

            for (var i = 0; i < alphastring.Length; i++)
                if (_learnLists.ElementAt(i) == null)
                    throw new IOException("Warning : alphabet in " + path + " is not complete");
        }
        
        public override RecognizedChar Recognize(Character chr)
        {
            var tested = chr.ExtractFeatures();
            var minx = 0;
            var minfx = float.PositiveInfinity;
            var recognized = new RecognizedChar();
            for (var x = 0; x < _learnLists.Count; x++)
            {
                var fx = SimplifiedEuclideanDistance(tested, _learnLists.ElementAt(x));
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
