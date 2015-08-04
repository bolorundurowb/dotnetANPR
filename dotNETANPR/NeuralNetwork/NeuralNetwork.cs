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
using System.Xml;
using System.IO;
using System.Text;

namespace dotNETANPR.NeuralNetwork
{
    class NeuralNetwork
    {
        private List<NeuralLayer> listLayers = new List<NeuralLayer>();
        private Random randomGenerator;

        public NeuralNetwork(List<Int32> dimensions)
        {
            for (int i = 0; i < dimensions.Count; i++)
            {
                this.listLayers.Add(new NeuralLayer(Convert.ToInt32(dimensions[i]), this));
            }
            randomGenerator = new Random();
        }

        public NeuralNetwork(string fileName)
        {
            LoadFromXml(fileName);
            randomGenerator = new Random();
        }

        public List<Double> Test(List<Double> inputs)
        {
            if (inputs.Count != this.GetLayer(0).NumberOfNeurons) 
            {
                throw new IndexOutOfRangeException("[Error] NN-Test: You are trying to pass vector with " + inputs.Count + " values into neural layer with " + this.GetLayer(0).NumberOfNeurons + " neurons. Consider using another network,  or another descriptors.");
            }
            else 
            return Activities(inputs);
        }

        public void Learn(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
        {
            if (trainingSet.pairs.Count == 0)
                throw new NullReferenceException("[Error] NN-Learn: You are using an empty training set,  neural network couldn't be trained.");
            else if (trainingSet.pairs[0].inputs.Count != this.GetLayer(0).NumberOfNeurons)
                throw new IndexOutOfRangeException("[Error] NN-Test: You are trying to pass vector with " + trainingSet.pairs[0].inputs.Count + " values into neural layer with " + this.GetLayer(0).NumberOfNeurons + " neurons. Consider using another network,  or another descriptors.");
            else if (trainingSet.pairs[0].outputs.Count != this.GetLayer(this.NumberOfLayers - 1).NumberOfNeurons)
                throw new IndexOutOfRangeException("[Error] NN-Test:  You are trying to pass vector with " + trainingSet.pairs[0].inputs.Count + " values into neural layer with " + this.GetLayer(0).NumberOfNeurons + " neurons. Consider using another network,  or another descriptors.");
            else 
                Adaptation(trainingSet, maxK, eps, lambda, micro);
        }

        public int NumberOfLayers
        {
            get
            {
                return this.listLayers.Count;
            }
        }

        private void LoadFromXml(string fileName)
        {
            Console.WriteLine("NeuralNetwork : loading network topology from file " + fileName);
            //DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance(); 
            //DocumentBuilder parser = factory.newDocumentBuilder(); 
            //Document doc = parser.parse(fileName); 

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode nodeNeuralNetwork = doc.DocumentElement;
            if (!nodeNeuralNetwork.Name.Equals("neuralNetwork")) throw new ApplicationException("[Error] NN-Load: Parse error in XML file,  neural network couldn't be loaded.");
            // nodeNeuralNetwork ok
            // indexNeuralNetworkContent -> indexStructureContent -> indexLayerContent -> indexNeuronContent -> indexNeuralInputContent
            XmlNodeList nodeNeuralNetworkContent = nodeNeuralNetwork.ChildNodes;
            for (int innc = 0; innc < nodeNeuralNetworkContent.Count; innc++)
            {
                XmlNode nodeStructure = nodeNeuralNetworkContent.Item(innc);
                if (nodeStructure.Name.Equals("structure"))
                { // for structure element
                    XmlNodeList nodeStructureContent = nodeStructure.ChildNodes;
                    for (int isc = 0; isc < nodeStructureContent.Count; isc++)
                    {
                        XmlNode nodeLayer = nodeStructureContent.Item(isc);
                        if (nodeLayer.Name.Equals("layer"))
                        { // for layer element
                            NeuralLayer neuralLayer = new NeuralLayer(this);
                            this.listLayers.Add(neuralLayer);
                            XmlNodeList nodeLayerContent = nodeLayer.ChildNodes;
                            for (int ilc = 0; ilc < nodeLayerContent.Count; ilc++)
                            {
                                XmlNode nodeNeuron = nodeLayerContent.Item(ilc);
                                if (nodeNeuron.Name.Equals("neuron"))
                                { // for neuron in layer
                                    Neuron neuron = new Neuron(Double.Parse(((XmlElement)nodeNeuron).GetAttribute("threshold")), neuralLayer);
                                    neuralLayer.listNeurons.Add(neuron);
                                    XmlNodeList nodeNeuronContent = nodeNeuron.ChildNodes;
                                    for (int inc = 0; inc < nodeNeuronContent.Count; inc++)
                                    {
                                        XmlNode nodeNeuralInput = nodeNeuronContent.Item(inc);
                                        //if (nodeNeuralInput==null) System.out.print("-"); else System.out.print("*"); 

                                        if (nodeNeuralInput.Name.Equals("input"))
                                        {
                                            //                                        Console.WriteLine("neuron at STR:" + innc+" LAY:" + isc+" NEU:" + ilc+" INP:" + inc); 
                                            NeuralInput neuralInput = new NeuralInput(Double.Parse(((XmlElement)nodeNeuralInput).GetAttribute("weight")), neuron);
                                            neuron.listInputs.Add(neuralInput);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SaveToXml(string fileName)
        {
            Console.WriteLine("Saving network topology to file " + fileName);
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("neuralNetwork");
            root.SetAttribute("dateOfExport", new DateTime().Date.ToShortDateString());
            XmlElement layers = doc.CreateElement("structure");
            layers.SetAttribute("numberOfLayers", this.NumberOfLayers.ToString());

            for (int il = 0; il < this.NumberOfLayers; il++)
            {
                XmlElement layer = doc.CreateElement("layer");
                layer.SetAttribute("index", il.ToString());
                layer.SetAttribute("numberOfNeurons", this.GetLayer(il).NumberOfNeurons.ToString());

                for (int ini = 0; ini < this.GetLayer(il).NumberOfNeurons; ini++)
                {
                    XmlElement neuron = doc.CreateElement("neuron");
                    neuron.SetAttribute("index", ini.ToString());
                    neuron.SetAttribute("NumberOfInputs", this.GetLayer(il).GetNeuron(ini).NumberOfInputs.ToString());
                    neuron.SetAttribute("threshold", this.GetLayer(il).GetNeuron(ini).threshold.ToString());

                    for (int ii = 0; ii < this.GetLayer(il).GetNeuron(ini).NumberOfInputs; ii++)
                    {
                        XmlElement input = doc.CreateElement("input");
                        input.SetAttribute("index", ii.ToString());
                        input.SetAttribute("weight", this.GetLayer(il).GetNeuron(ini).GetInput(ii).weight.ToString());

                        neuron.AppendChild(input);
                    }
                    layer.AppendChild(neuron);
                }
                layers.AppendChild(layer);
            }

            root.AppendChild(layers);
            doc.AppendChild(root);

            // save
            FileStream fs = File.Create(fileName);
            XmlWriter transformer;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.GetEncoding(28592);
            transformer = XmlWriter.Create(fs, settings);
            doc.Save(transformer);
        }

        private double Random()
        {
            return randomGenerator.NextDouble();
        }

        private List<Double> Activities(List<Double> inputs)
        {
            for (int il = 0; il < this.NumberOfLayers; il++)
            {
                for (int ini = 0; ini < this.GetLayer(il).NumberOfNeurons; ini++)
                { 
                    double sum = this.GetLayer(il).GetNeuron(ini).threshold; 
                    for (int ii = 0; ii < this.GetLayer(il).GetNeuron(ini).NumberOfInputs; ii++)
                    {
                        if (il == 0)
                        { 
                            sum += this.GetLayer(il).GetNeuron(ini).GetInput(ii).weight *
                            inputs[ini];
                        }
                        else
                        { 
                            sum +=
                            this.GetLayer(il).GetNeuron(ini).GetInput(ii).weight *
                            this.GetLayer(il - 1).GetNeuron(ii).output;
                        }
                    }


                    this.GetLayer(il).GetNeuron(ini).output = this.GainFunction(sum); 
                }
            }
            List<Double> output = new List<Double>();
            for (int i = 0; i < this.GetLayer(this.NumberOfLayers - 1).NumberOfNeurons; i++)
                output.Add(this.GetLayer(this.NumberOfLayers - 1).GetNeuron(i).output);
            return output;
        }


        private void ComputeGradient(Gradients gradients, List<Double> inputs, List<Double> requiredOutputs)
        {
            Activities(inputs);
            for (int il = this.NumberOfLayers - 1; il >= 1; il--)
            {
                NeuralLayer currentLayer = this.GetLayer(il);
                if (currentLayer.IsLayerTop)
                {
                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    {
                        Neuron currentNeuron = currentLayer.GetNeuron(ini);
                        gradients.SetThreshold(il, ini, currentNeuron.output * (1 - currentNeuron.output) * (currentNeuron.output - requiredOutputs[ini]));
                    }

                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    {
                        // for each neuron
                        Neuron currentNeuron = currentLayer.GetNeuron(ini);
                        for (int ii = 0; ii < currentNeuron.NumberOfInputs; ii++)
                        { // for each neuron's input
                            NeuralInput currentInput = currentNeuron.GetInput(ii);
                            gradients.SetWeight(il, ini, ii,
                                    gradients.GetThreshold(il, ini) * currentLayer.LowerLayer().GetNeuron(ii).output
                                    );
                        } 
                    } 
                }
                else
                {
                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    {
                        double aux = 0;
                        for (int ia = 0; ia < currentLayer.UpperLayer().NumberOfNeurons; ia++)
                        {
                            aux += gradients.GetThreshold(il + 1, ia) *
                                   currentLayer.UpperLayer().GetNeuron(ia).GetInput(ini).weight;
                        }
                        gradients.SetThreshold(il, ini,
                                currentLayer.GetNeuron(ini).output * (1 - currentLayer.GetNeuron(ini).output) * aux
                                );
                    } 
                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    { 
                        Neuron currentNeuron = currentLayer.GetNeuron(ini);
                        for (int ii = 0; ii < currentNeuron.NumberOfInputs; ii++)
                        { 
                            NeuralInput currentInput = currentNeuron.GetInput(ii);
                            gradients.SetWeight(il, ini, ii,
                                    gradients.GetThreshold(il, ini) * currentLayer.LowerLayer().GetNeuron(ii).output
                            );
                        } 
                    } 
                } 
            }
        }

        private void ComputeTotalGradient(Gradients totalGradients, Gradients partialGradients, SetOfIOPairs trainingSet)
        {
            totalGradients.ResetGradients();
            foreach (SetOfIOPairs.IOPair pair in trainingSet.pairs)
            {
                ComputeGradient(partialGradients, pair.inputs, pair.outputs);
                for (int il = this.NumberOfLayers - 1; il >= 1; il--)
                {
                    NeuralLayer currentLayer = this.GetLayer(il);
                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    {

                        totalGradients.IncrementThreshold(il, ini, partialGradients.GetThreshold(il, ini));
                        for (int ii = 0; ii < currentLayer.LowerLayer().NumberOfNeurons; ii++)
                        { // pre vsetky vstupy
                            totalGradients.IncrementWeight(il, ini, ii, partialGradients.GetWeight(il, ini, ii));
                        }
                    }
                } 
            } 
        } 

        private void Adaptation(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
        {   
            double delta;
            Gradients deltaGradients = new Gradients(this);
            Gradients totalGradients = new Gradients(this);
            Gradients partialGradients = new Gradients(this);

            Console.WriteLine("setting up random weights and thresholds ...");
            for (int il = this.NumberOfLayers - 1; il >= 1; il--)
            { // iteracia cez vsetky vrstvy nadol okrem poslednej
                NeuralLayer currentLayer = this.GetLayer(il);
                for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                {
                    Neuron currentNeuron = currentLayer.GetNeuron(ini);
                    currentNeuron.threshold = 2 * this.Random() - 1;
                    for (int ii = 0; ii < currentNeuron.NumberOfInputs; ii++)
                    {
                        currentNeuron.GetInput(ii).weight = 2 * this.Random() - 1;
                    }
                }
            }

            int currK = 0; 
            double currE = Double.PositiveInfinity;
            Console.WriteLine("entering adaptation loop ... (maxK = " + maxK + ")");

            while (currK < maxK && currE > eps)
            {
                ComputeTotalGradient(totalGradients, partialGradients, trainingSet);
                for (int il = this.NumberOfLayers - 1; il >= 1; il--)
                {
                    NeuralLayer currentLayer = this.GetLayer(il);

                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    {
                        Neuron currentNeuron = currentLayer.GetNeuron(ini);
                        delta = -lambda * totalGradients.GetThreshold(il, ini) + micro * deltaGradients.GetThreshold(il, ini);
                        currentNeuron.threshold += delta;
                        deltaGradients.SetThreshold(il, ini, delta);
                    }

                    for (int ini = 0; ini < currentLayer.NumberOfNeurons; ini++)
                    {
                        Neuron currentNeuron = currentLayer.GetNeuron(ini);
                        for (int ii = 0; ii < currentNeuron.NumberOfInputs; ii++)
                        {
                            delta = -lambda * totalGradients.GetWeight(il, ini, ii) + micro * deltaGradients.GetWeight(il, ini, ii);
                            currentNeuron.GetInput(ii).weight += delta;
                            deltaGradients.SetWeight(il, ini, ii, delta);
                        }
                    }
                }

                currE = totalGradients.GetGradientAbs();
                currK++;
                if (currK % 25 == 0) Console.WriteLine("currK=" + currK + "   currE=" + currE);
            } 
        }

        private double GainFunction(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        private NeuralLayer GetLayer(int index)
        {
            return this.listLayers[index];
        }

        public class NeuralInput
        {
            public double weight;
            int index;
            Neuron neuron;

            public NeuralInput(double weight, Neuron neuron)
            {
                this.neuron = neuron;
                this.weight = weight;
                this.index = this.neuron.NumberOfInputs;
            }
        }

        public class SetOfIOPairs
        {
            public List<IOPair> pairs;

            public class IOPair
            { 
                public List<Double> inputs;
                public List<Double> outputs;

                public IOPair(List<Double> inputs, List<Double> outputs)
                {
                    this.inputs = new List<Double>(inputs);
                    this.outputs = new List<Double>(outputs);
                }
            }

            public SetOfIOPairs()
            {
                this.pairs = new List<IOPair>();
            }

            public void AddIOPair(List<Double> inputs, List<Double> outputs)
            {
                this.AddIOPair(new IOPair(inputs, outputs));
            }

            public void AddIOPair(IOPair pair)
            {
                this.pairs.Add(pair);
            }

            public int Count
            {
                get
                {
                    return pairs.Count;
                }
            }
        }

        public class Neuron
        {
            private List<NeuralInput> listInputs = new List<NeuralInput>(); 
            int index;
            public double threshold;
            public double output;
            NeuralLayer neuralLayer;

            public Neuron(double threshold, NeuralLayer neuralLayer)
            {
                this.threshold = threshold;
                this.neuralLayer = neuralLayer;
                this.index = this.neuralLayer.NumberOfNeurons;
            }

            public Neuron(int numberOfInputs, double threshold, NeuralLayer neuralLayer)
            {
                this.threshold = threshold;
                this.neuralLayer = neuralLayer;
                this.index = this.neuralLayer.NumberOfNeurons;
                for (int i = 0; i < numberOfInputs; i++)
                {
                    this.listInputs.Add(new NeuralInput(1.0, this));
                }
            }

            public int NumberOfInputs
            {
                get
                {
                    return this.listInputs.Count;
                }
            }

            public NeuralInput GetInput(int index)
            {
                return this.listInputs[index];
            }

        }

        public class NeuralLayer
        {
            public List<Neuron> listNeurons = new List<Neuron>();
            int index;
            NeuralNetwork neuralNetwork;

            public NeuralLayer(NeuralNetwork neuralNetwork)
            {
                this.neuralNetwork = neuralNetwork;
                this.index = this.neuralNetwork.NumberOfLayers;
            }

            public NeuralLayer(int numberOfNeurons, NeuralNetwork neuralNetwork)
            {
                this.neuralNetwork = neuralNetwork;
                this.index = this.neuralNetwork.NumberOfLayers;
                for (int i = 0; i < numberOfNeurons; i++)
                {
                    if (this.index == 0)
                    {
                        this.listNeurons.Add(new Neuron(1, 0.0, this));
                    }
                    else
                    {
                        this.listNeurons.Add(new Neuron(this.neuralNetwork.GetLayer(this.index - 1).NumberOfNeurons, 0.0, this));
                    }
                }
            }

            public int NumberOfNeurons
            {
                get
                {
                    return this.listNeurons.Count;
                }
            }

            public bool IsLayerTop
            {
                get
                {
                    return (this.index == this.neuralNetwork.NumberOfLayers - 1);
                }
            }

            public bool IsLayerBottom
            {
                get
                {
                    return (this.index == 0);
                }
            }

            public NeuralLayer UpperLayer()
            {
                if (this.IsLayerTop) 
                    return null;
                return 
                    this.neuralNetwork.GetLayer(index + 1);
            }

            public NeuralLayer LowerLayer()
            {
                if (this.IsLayerBottom) 
                    return null;
                return 
                    this.neuralNetwork.GetLayer(index - 1);
            }

            public Neuron GetNeuron(int index)
            {
                return this.listNeurons[index];
            }
        }

        private class Gradients
        {
            List<List<Double>> thresholds;
            List<List<List<Double>>> weights;
            NeuralNetwork neuralNetwork;

            public Gradients(NeuralNetwork network)
            {
                this.neuralNetwork = network;
                this.InitGradients();
            }

            public void InitGradients()
            {
                this.thresholds = new List<List<Double>>();
                this.weights = new List<List<List<Double>>>();
                for (int il = 0; il < this.neuralNetwork.NumberOfLayers; il++)
                {
                    this.thresholds.Add(new List<Double>());
                    this.weights.Add(new List<List<Double>>());
                    for (int ini = 0; ini < this.neuralNetwork.GetLayer(il).NumberOfNeurons; ini++)
                    {
                        this.thresholds[il].Add(0.0);
                        this.weights[il].Add(new List<Double>());
                        for (int ii = 0; ii < this.neuralNetwork.GetLayer(il).GetNeuron(ini).NumberOfInputs; ii++)
                        {
                            this.weights[il][ini].Add(0.0);
                        }
                    }
                }
            }

            public void ResetGradients()
            { //resets to 0
                for (int il = 0; il < this.neuralNetwork.NumberOfLayers; il++)
                {
                    for (int ini = 0; ini < this.neuralNetwork.GetLayer(il).NumberOfNeurons; ini++)
                    {
                        this.SetThreshold(il, ini, 0.0);
                        for (int ii = 0; ii < this.neuralNetwork.GetLayer(il).GetNeuron(ini).NumberOfInputs; ii++)
                        {
                            this.SetWeight(il, ini, ii, 0.0);
                        }
                    }
                }
            }

            public double GetThreshold(int il, int ini)
            {
                return Double.Parse(thresholds[il][ini].ToString());
            }

            public void SetThreshold(int il, int ini, double value)
            {
                thresholds[il][ini] = value;
            }

            public void IncrementThreshold(int il, int ini, double value)
            {
                this.SetThreshold(il, ini, this.GetThreshold(il, ini) + value);
            }

            public double GetWeight(int il, int ini, int ii)
            {
                return weights[il][ini][ii];
            }

            public void SetWeight(int il, int ini, int ii, double value)
            {
                weights[il][ini][ii] = value;
            }

            public void IncrementWeight(int il, int ini, int ii, double value)
            {
                this.SetWeight(il, ini, ii, this.GetWeight(il, ini, ii) + value);
            }

            public double GetGradientAbs()
            {
                double currE = 0;

                for (int il = 1; il < neuralNetwork.NumberOfLayers; il++)
                {
                    currE += this.VectorAbs(thresholds[il]);
                    currE += this.DoubleVectorAbs(weights[il]);
                }
                return currE;
            }

            private double DoubleVectorAbs(List<List<Double>> doubleVector)
            {
                double totalX = 0;
                foreach (List<Double> vector in doubleVector)
                {
                    totalX += Math.Pow(VectorAbs(vector), 2);
                }
                return Math.Sqrt(totalX);
            }

            private double VectorAbs(List<Double> vector)
            {
                double totalX = 0;
                foreach (Double x in vector)
                {
                    totalX += Math.Pow(x, 2);
                }
                return Math.Sqrt(totalX);
            }
        }
    }
}
