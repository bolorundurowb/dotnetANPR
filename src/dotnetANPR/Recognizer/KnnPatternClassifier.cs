using System;
using DotNetANPR.Config;
using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.Recognizer
{
    public class KnnPatternClassificator : CharacterRecognizer, ICharacterRecognizer
    {
        public KnnPatternClassificator(AppSettings settings) : base(settings) { }

        public RecognizedChar Recognize(LicensePlateChar chr)
        {
            var recognizedChar = new RecognizedChar();
            float[] featureVector = chr.GetFeatureVector();

            foreach (var (character, alphabetVector) in _alphabet)
            {
                double distance = GetEuclideanDistance(featureVector, alphabetVector);
                recognizedChar.AddPattern(new RecognizedPattern(character, distance));
            }
            
            recognizedChar.Sort();
            return recognizedChar;
        }

        private double GetEuclideanDistance(float[] v1, float[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += Math.Pow(v1[i] - v2[i], 2);
            }
            return Math.Sqrt(sum);
        }
    }
}