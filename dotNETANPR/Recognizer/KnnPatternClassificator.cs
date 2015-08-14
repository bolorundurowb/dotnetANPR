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
                float fx = this.SimplifiedEuclideanDistance(tested, this.learnLists.ElementAt(x));

                recognized.AddPattern(new RecognizedChar.RecognizedPattern(this.alphabet[x], fx));
            }
            recognized.Sort(0);
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
