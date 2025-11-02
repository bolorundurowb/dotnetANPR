using DotNetANPR.Config;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.NeuralNetwork; // Assuming this namespace

namespace DotNetANPR.Recognizer
{
    public class NeuralPatternClassificator : CharacterRecognizer, ICharacterRecognizer
    {
        private readonly AnprNeuralNetwork _network;

        public NeuralPatternClassificator(AppSettings settings) : base(settings)
        {
            string networkPath = settings.Recognition.CharNeuralNetworkPath;
            string mapPath = settings.Recognition.CharLearnAlphabetPath; // Re-using for map
            _network = new AnprNeuralNetwork(networkPath, mapPath);
        }

        public RecognizedChar Recognize(LicensePlateChar chr)
        {
            float[] featureVector = chr.GetFeatureVector();
            var output = _network.Compute(featureVector);
            
            var recognizedChar = new RecognizedChar();
            for(int i=0; i < output.Length; i++)
            {
                if (_network.Map.TryGetValue(i, out char character))
                {
                    // For NN, similarity is inverse of probability (lower is better)
                    recognizedChar.AddPattern(new RecognizedPattern(character, 1.0 - output[i]));
                }
            }
            
            recognizedChar.Sort();
            return recognizedChar;
        }
    }
}