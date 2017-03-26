using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotNETANPR.ImageAnalysis;

namespace dotNETANPR.Recognizer
{
    public class NeuralPatternClassificator : CharacterRecognizer, ICharacterRecognizer
    {
        private static readonly int NormalizeX =
            Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_x");

        private static readonly int NormalizeY =
            Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_y");

        public NeuralNetwork.NeuralNetwork Network;

        public NeuralPatternClassificator() : this(false)
        {
        }

        public NeuralPatternClassificator(bool learn)
        {
            List<int> dimensions = new List<int>();
            int inputLayerSize;
            if (Intelligence.Intelligence.Configurator.GetIntProperty("char_featuresExtractionMethod") == 0)
                inputLayerSize = NormalizeX * NormalizeY;
            else inputLayerSize = features.Length * 4;

            dimensions.Add(inputLayerSize);
            dimensions.Add(Intelligence.Intelligence.Configurator.GetIntProperty("neural_topology"));
            dimensions.Add(Alphabet.Length);
            Network = new NeuralNetwork.NeuralNetwork(dimensions);

            if (learn)
            {
                LearnAlphabet(Intelligence.Intelligence.Configurator.GetStringProperty("char_learnAlphabetPath"));
            }
            else
            {
                Network = new NeuralNetwork.NeuralNetwork(
                    Intelligence.Intelligence.Configurator.GetPathProperty("char_neuralNetworkPath"));
            }
        }

        public RecognizedChar Recognize(Character imgChar)
        {
            imgChar.Normalize();
            List<double> output = Network.Test(imgChar.ExtractFeatures());
            RecognizedChar recognized = new RecognizedChar();
            for (int i = 0; i < output.Count; i++)
            {
                recognized.AddPattern(new RecognizedChar.RecognizedPattern(Alphabet[i], (float) output[i]));
            }
            recognized.Render();
            recognized.Sort(1);
            return recognized;
        }

        public NeuralNetwork.NeuralNetwork.SetOfIOPairs.IOPair CreateNewPair(char chr, Character imgChar)
        {
            List<double> vectorInput = imgChar.ExtractFeatures();
            List<double> vectorOutput = Alphabet.Select(t => chr == t ? 1.0 : 0.0).ToList();
            return (new NeuralNetwork.NeuralNetwork.SetOfIOPairs.IOPair(vectorInput, vectorOutput));
        }

        public void LearnAlphabet(string path)
        {
            string alphastring = "0123456789abcdefghijklmnopqrstuvwxyz";
            string[] files = Directory.GetFiles(path);
            NeuralNetwork.NeuralNetwork.SetOfIOPairs train = new NeuralNetwork.NeuralNetwork.SetOfIOPairs();
            foreach (string fileName in files)
            {
                if (alphastring.IndexOf(fileName.ToLower()[0]) == -1)
                    continue;

                Character imgChar = new Character(path + Path.DirectorySeparatorChar + fileName);
                imgChar.Normalize();
                train.AddIOPair(CreateNewPair(fileName.ToUpper()[0], imgChar));
            }

            Network.Learn(train,
                Intelligence.Intelligence.Configurator.GetIntProperty("neural_maxk"),
                Intelligence.Intelligence.Configurator.GetDoubleProperty("neural_eps"),
                Intelligence.Intelligence.Configurator.GetDoubleProperty("neural_lambda"),
                Intelligence.Intelligence.Configurator.GetDoubleProperty("neural_micro")
            );
        }
    }
}
