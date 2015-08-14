/*
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dotNETANPR.Recognizer
{
    class NeuralPatternClassificator : CharacterRecognizer
    {
        static Configurator.Configurator config = new Configurator.Configurator();
        private static int normalize_x =
            config.GetIntProperty("char_normalizeddimensions_x");
        private static int normalize_y =
                config.GetIntProperty("char_normalizeddimensions_y");

        public NeuralNetwork.NeuralNetwork Network;

        public NeuralPatternClassificator()
        {
            new NeuralPatternClassificator(false);
        }

        public NeuralPatternClassificator(bool learn)
        {
            List<Int32> dimensions = new List<Int32>();
            int inputLayerSize;
            if (config.GetIntProperty("char_featuresExtractionMethod") == 0)
                inputLayerSize = normalize_x * normalize_y;
            else inputLayerSize = CharacterRecognizer.features.Length * 4;
            
            dimensions.Add(inputLayerSize);
            dimensions.Add(config.GetIntProperty("neural_topology"));
            dimensions.Add(CharacterRecognizer.alphabet.Length);
            Network = new NeuralNetwork.NeuralNetwork(dimensions);

            if (learn)
            {
                LearnAlphabet(config.GetStrProperty("char_learnAlphabetPath"));
            }
            else
            {
                Network = new NeuralNetwork.NeuralNetwork(config.GetPathProperty("char_neuralNetworkPath"));
            }
        }

        public RecognizedChar Recognize(ImageAnalysis.Char imgChar)
        {
            imgChar.Normalize();
            List<Double> output = this.Network.Test(imgChar.ExtractFeatures());
            double max = 0.0;
            int indexMax = 0;

            RecognizedChar recognized = new RecognizedChar();

            for (int i = 0; i < output.Count; i++)
            {
                recognized.AddPattern(new RecognizedChar.RecognizedPattern(alphabet[i], (float)output.ElementAt(i)));
            }
            recognized.Render();
            recognized.Sort(1);
            return recognized;
        }

        public NeuralNetwork.NeuralNetwork.SetOfIOPairs.IOPair CreateNewPair(char chr, ImageAnalysis.Char imgChar)
        {
            List<Double> vectorInput = imgChar.ExtractFeatures();
            List<Double> vectorOutput = new List<Double>();
            for (int i = 0; i < alphabet.Length; i++)
                if (chr == alphabet[i]) vectorOutput.Add(1.0); else vectorOutput.Add(0.0);
            return (new NeuralNetwork.NeuralNetwork.SetOfIOPairs.IOPair(vectorInput, vectorOutput));
        }

        public void LearnAlphabet(String path)
        {
            String alphaString = "0123456789abcdefghijklmnopqrstuvwxyz";
            string[] files = Directory.GetFiles(path);
            NeuralNetwork.NeuralNetwork.SetOfIOPairs train = new NeuralNetwork.NeuralNetwork.SetOfIOPairs();

            foreach (String fileName in files)
            {
                if (alphaString.IndexOf(fileName.ToLower()[0]) == -1)
                    continue;

                ImageAnalysis.Char imgChar = new ImageAnalysis.Char(path + Path.DirectorySeparatorChar + fileName);
                imgChar.Normalize();
                train.AddIOPair(this.CreateNewPair(fileName.ToUpper()[0], imgChar));
            }

            this.Network.Learn(train,
                    config.GetIntProperty("neural_maxk"),
                    config.GetDoubleProperty("neural_eps"),
                    config.GetDoubleProperty("neural_lambda"),
                    config.GetDoubleProperty("neural_micro")
                    );
        }
    }
}
