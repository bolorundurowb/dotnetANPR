﻿using System;
using System.Collections.Generic;
 using System.Xml;

namespace dotnetANPR.NeuralNetwork
{
    public class NeuralNetwork
    {
        private List<NeuralLayer> listLayers = new List<NeuralLayer>();
        private Random _random;

        public NeuralNetwork(List<int> dimensions)
        {
            for (var i = 0; i < dimensions.Count; i++)
            {
                listLayers.Add(new NeuralLayer(dimensions[i], this));
            }
            _random = new Random();
        }

        public NeuralNetwork(string filePath)
        {
            LoadFromXml(filePath);
            _random = new Random();
        }

        public class SetOfIOPairs
        {
            public int Count
            {
                get { return Pairs.Count; }
            }

            public List<IOPair> Pairs { get; }

            public class IOPair
            {
                public List<double> Inputs { get; set; }
                public List<double> Outputs { get; set; }

                public IOPair(List<double> inputs, List<double> outputs)
                {
                    Inputs = new List<double>(inputs);
                    Outputs = new List<double>(outputs);
                }
            }

            public SetOfIOPairs()
            {
                Pairs = new List<IOPair>();
            }

            public void AddIOPair(List<double> inputs, List<double> outputs)
            {
                AddIOPair(new IOPair(inputs, outputs));
            }

            public void AddIOPair(IOPair ioPair)
            {
                Pairs.Add(ioPair);
            }
        }

        class NeuralInput
        {
            public double Weight { get; set; }
            public int Index { get; set; }
            public Neuron Neuron { get; set; }

            public NeuralInput(double weight, Neuron neuron)
            {
                Weight = weight;
                Index = neuron.NumberOfInputs();
                Neuron = neuron;
            }
        }

        class Neuron
        {
            public List<NeuralInput> listInputs = new List<NeuralInput>();
            public int Index { get; set; }
            public double Threshold { get; set; }
            public double Output { get; set; }
            public NeuralLayer NeuralLayer { get; set; }

            public Neuron(double threshold, NeuralLayer neuralLayer)
            {
                Threshold = threshold;
                NeuralLayer = neuralLayer;
                Index = NeuralLayer.NumberOfNeurons();
            }

            public Neuron(int numberOfInputs, double threshold, NeuralLayer neuralLayer)
            {
                Threshold = threshold;
                NeuralLayer = neuralLayer;
                Index = NeuralLayer.NumberOfNeurons();
                for (var i = 0; i < numberOfInputs; i++)
                {
                    listInputs.Add(new NeuralInput(1.0, this));
                }
            }

            public int NumberOfInputs()
            {
                return listInputs.Count;
            }

            public NeuralInput GetInput(int index)
            {
                return listInputs[index];
            }
        }

        class NeuralLayer
        {
            public List<Neuron> listNeurons = new List<Neuron>();
            public int Index { get; set; }
            public NeuralNetwork NeuralNetwork { get; set; }

            public NeuralLayer(NeuralNetwork neuralNetwork)
            {
                NeuralNetwork = neuralNetwork;
                Index = neuralNetwork.NumberOfLayers();
            }

            public NeuralLayer(int numberOfNeurons, NeuralNetwork neuralNetwork)
            {
                NeuralNetwork = neuralNetwork;
                Index = neuralNetwork.NumberOfLayers();
                for (var i = 0; i < numberOfNeurons; i++)
                {
                    if (Index == 0)
                    {
                        listNeurons.Add(new Neuron(1, 0.0, this));
                    }
                    else
                    {
                        listNeurons.Add(
                            new Neuron(NeuralNetwork.GetLayer(Index - 1).NumberOfNeurons(), 0.0, this)
                        );
                    }
                }
            }

            public int NumberOfNeurons()
            {
                return listNeurons.Count;
            }

            public bool IsLayerTop()
            {
                return Index == NeuralNetwork.NumberOfLayers() - 1;
            }

            public bool IsLayerBottom()
            {
                return Index == 0;
            }

            public NeuralLayer UpperLayer()
            {
                if (IsLayerTop()) return null;
                return NeuralNetwork.GetLayer(Index + 1);
            }

            public NeuralLayer LowerLayer()
            {
                if (IsLayerBottom()) return null;
                return NeuralNetwork.GetLayer(Index - 1);
            }

            public Neuron GetNeuron(int index)
            {
                return listNeurons[index];
            }
        }

        class Gradients
        {
            private List<List<double>> thresholds;
            private List<List<List<double>>> weights;
            public NeuralNetwork NeuralNetwork { get; set; }

            public Gradients(NeuralNetwork neuralNetwork)
            {
                NeuralNetwork = neuralNetwork;
                InitGradients();
            }

            private void InitGradients()
            {
                thresholds = new List<List<double>>();
                weights = new List<List<List<double>>>();
                for (var i = 0; i < NeuralNetwork.NumberOfLayers(); i++)
                {
                    thresholds.Add(new List<double>());
                    weights.Add(new List<List<double>>());
                    for (var j = 0; j < NeuralNetwork.GetLayer(i).NumberOfNeurons(); j++)
                    {
                        thresholds[i].Add(0.0);
                        weights[i].Add(new List<double>());
                        for (var k = 0; k < NeuralNetwork.GetLayer(i).GetNeuron(j).NumberOfInputs(); k++)
                        {
                            weights[i][j].Add(0.0);
                        }
                    }
                }
            }

            public void ResetGradients()
            {
                for (var i = 0; i < NeuralNetwork.NumberOfLayers(); i++)
                {
                    for (var j = 0; j < NeuralNetwork.GetLayer(i).NumberOfNeurons(); j++)
                    {
                        SetThreshold(i, j, 0.0);
                        for (var k = 0; k < NeuralNetwork.GetLayer(i).GetNeuron(j).NumberOfInputs(); k++)
                        {
                            SetWeight(i, j, k, 0.0);
                        }
                    }
                }
            }

            public double GetThreshold(int i, int j)
            {
                return thresholds[i][j];
            }

            public void SetThreshold(int i, int j, double value)
            {
                thresholds[i][j] = value;
            }

            public void IncrementThreshold(int i, int j, double value)
            {
                SetThreshold(i, j, GetThreshold(i, j) + value);
            }

            public double GetWeight(int i, int j, int k)
            {
                return weights[i][j][k];
            }

            public void SetWeight(int i, int j, int k, double value)
            {
                weights[i][j][k] = value;
            }

            public void IncrementWeight(int i, int j, int k, double value)
            {
                SetWeight(i, j, k, GetWeight(i, j, k) + value);
            }

            public double GetGradientAbs()
            {
                double currE = 0;
                for (var i = 1; i < NeuralNetwork.NumberOfLayers(); i++)
                {
                    currE += VectorAbs(thresholds[i]);
                    currE += DoubleVectorAbs(weights[i]);
                }
                return currE;
            }

            private double DoubleVectorAbs(List<List<double>> doubleList)
            {
                double totalX = 0;
                foreach (var list in doubleList)
                {
                    totalX += Math.Pow(VectorAbs(list), 2);
                }
                return Math.Sqrt(totalX);
            }

            private double VectorAbs(List<double> doubles)
            {
                double totalX = 0;
                foreach (var x in doubles)
                {
                    totalX += Math.Pow(x, 2);
                }
                return Math.Sqrt(totalX);
            }
        }

        private double Random()
        {
            return _random.NextDouble();
        }

        private NeuralLayer GetLayer(int index)
        {
            return listLayers[index];
        }

        private double GainFunction(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        private void LoadFromXml(string filePath)
        {
            Console.WriteLine("NeuralNetwork : loading network topology from file " + filePath);
            // Read XML to XmlDocument
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var rootNodeName = xmlDocument.Name;
            if (rootNodeName != "neuralNetwork")
            {
                throw new InvalidOperationException(
                    "[Error] NN-Load: Parse error in XML file, neural network couldn't be loaded.");
            }
            var nodeList = xmlDocument.ChildNodes;
            for (var i = 0; i < nodeList.Count; i++)
            {
                var nodeStructure = nodeList.Item(i);
                if (nodeStructure != null && nodeStructure.Name == "structure")
                {
                    var nodeStructureContent = nodeStructure.ChildNodes;
                    for (var j = 0; j < nodeStructureContent.Count; j++)
                    {
                        var nodeLayer = nodeStructureContent.Item(j);
                        if (nodeLayer != null && nodeLayer.Name == "layer")
                        {
                            var neuralLayer = new NeuralLayer(this);
                            listLayers.Add(neuralLayer);

                            var nodeLayerContent = nodeLayer.ChildNodes;
                            for (var k = 0; k < nodeLayerContent.Count; k++)
                            {
                                var nodeNeuron = nodeLayerContent.Item(k);
                                if (nodeNeuron != null && nodeNeuron.Name == "neuron")
                                {
                                    var neuron = new Neuron(double.Parse(nodeNeuron.Attributes["threshold"].ToString()), neuralLayer);
                                    neuralLayer.listNeurons.Add(neuron);
                                    var nodeNeuronContent = nodeNeuron.ChildNodes;
                                    for (var l = 0; l < nodeNeuronContent.Count; l++)
                                    {
                                        var nodeNeuralInput = nodeNeuronContent.Item(l);
                                        if (nodeNeuralInput != null && nodeNeuralInput.Name == "input")
                                        {
                                            var neuralInput = new NeuralInput(double.Parse(nodeNeuralInput.Attributes["weight"].ToString()), neuron);
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

        public void SaveToXml(string filePath)
        {
            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("neuralNetwork");
            root.SetAttribute("dateOfExport", new DateTime().ToString());
            var layers = xmlDocument.CreateElement("structure");
            layers.SetAttribute("numberOfLayers", NumberOfLayers().ToString());
            for (var i = 0; i < NumberOfLayers(); i++)
            {
                var layer = xmlDocument.CreateElement("layer");
                layer.SetAttribute("index", i.ToString());
                layer.SetAttribute("numberOfNeutrons", GetLayer(i).NumberOfNeurons().ToString());

                for (var j = 0; j < GetLayer(i).NumberOfNeurons(); j++)
                {
                    var neuron = xmlDocument.CreateElement("neuron");
                    neuron.SetAttribute("index", j.ToString());
                    neuron.SetAttribute("NumberOfInputs", GetLayer(i).GetNeuron(j).NumberOfInputs().ToString());
                    neuron.SetAttribute("threshold", GetLayer(i).GetNeuron(j).Threshold.ToString());
                    for (var k = 0; k < GetLayer(i).GetNeuron(j).NumberOfInputs(); k++)
                    {
                        var input = xmlDocument.CreateElement("input");
                        input.SetAttribute("index", k.ToString());
                        input.SetAttribute("weight", GetLayer(i).GetNeuron(j).GetInput(k).Weight.ToString());
                        neuron.AppendChild(input);
                    }
                    layer.AppendChild(neuron);
                }
                layers.AppendChild(layer);
            }
            root.AppendChild(layers);
            xmlDocument.AppendChild(root);
            // Save XML
            xmlDocument.Save(filePath);
        }

        public List<double> Test(List<double> inputs)
        {
            if (inputs.Count != GetLayer(0).NumberOfNeurons())
            {
                throw new IndexOutOfRangeException("[Error] NN-Test: You are trying to pass vector with " +
                                                   inputs.Count + " values into neural layer with " +
                                                   GetLayer(0).NumberOfNeurons() +
                                                   " neurons. Consider using another network, or another descriptor.");
            }
            return Activities(inputs);
        }

        public void Learn(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
        {
            if (trainingSet.Pairs.Count == 0)
            {
                throw new NullReferenceException(
                    "[Error] NN-Learn: You are using an empty training set, neural network couldn't be trained.");
            }
            if (trainingSet.Pairs[0].Inputs.Count != GetLayer(0).NumberOfNeurons())
            {
                throw new IndexOutOfRangeException("[Error] NN-Test: You are trying to pass vector with " +
                                                   trainingSet.Pairs[0].Inputs.Count +
                                                   " values into neural layer with " +
                                                   GetLayer(0).NumberOfNeurons() +
                                                   " neurons. Consider using another network, or another descriptors.");
            }
            if (trainingSet.Pairs[0].Outputs.Count != GetLayer(NumberOfLayers() - 1).NumberOfNeurons())
            {
                throw new IndexOutOfRangeException("[Error] NN-Test:  You are trying to pass vector with " +
                                                   trainingSet.Pairs[0].Inputs.Count +
                                                   " values into neural layer with " +
                                                   GetLayer(0).NumberOfNeurons() +
                                                   " neurons. Consider using another network, or another descriptors.");
            }
            Adaptation(trainingSet, maxK, eps, lambda, micro);
        }

        public int NumberOfLayers()
        {
            return listLayers.Count;
        }

        private void ComputeGradient(Gradients gradients, List<double> inputs, List<double> requiredOutputs)
        {
            Activities(inputs);
            for (var i = NumberOfLayers() - 1; i >= 1; i--)
            {
                var currentLayer = GetLayer(i);

                if (currentLayer.IsLayerTop())
                {
                    for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                    {
                        var currentNeuron = currentLayer.GetNeuron(j);
                        gradients.SetThreshold(i, j,
                            currentNeuron.Output * (1 - currentNeuron.Output) *
                            (currentNeuron.Output - requiredOutputs[j]));
                    }

                    for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                    {
                        var currentNeuron = currentLayer.GetNeuron(j);
                        for (var k = 0; k < currentNeuron.NumberOfInputs(); k++)
                        {
                            var currentInput = currentNeuron.GetInput(k);
                            gradients.SetWeight(i, j, k,
                                gradients.GetThreshold(i, j) * currentLayer.LowerLayer().GetNeuron(k).Output);
                        }
                    }

                }
                else
                {
                    for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                    {
                        double aux = 0;
                        for (var ia = 0; ia < currentLayer.UpperLayer().NumberOfNeurons(); ia++)
                        {
                            aux += gradients.GetThreshold(i + 1, ia) *
                                   currentLayer.UpperLayer().GetNeuron(ia).GetInput(j).Weight;
                        }
                        gradients.SetThreshold(i, j,
                            currentLayer.GetNeuron(j).Output * (1 - currentLayer.GetNeuron(j).Output) * aux);
                    }

                    for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                    {
                        var currentNeuron = currentLayer.GetNeuron(j)
                        ;
                        for (var k = 0; k < currentNeuron.NumberOfInputs(); k++)
                        {
                            var currentInput = currentNeuron.GetInput(k);
                            gradients.SetWeight(i, j, k,
                                gradients.GetThreshold(i, j) * currentLayer.LowerLayer().GetNeuron(k).Output);
                        }
                    }

                }

            }
        }

        private void ComputeTotalGradient(Gradients totalGradients, Gradients partialGradients,
            SetOfIOPairs trainingSet)
        {
            totalGradients.ResetGradients();

            foreach (var pair in trainingSet.Pairs)
            {
                ComputeGradient(partialGradients, pair.Inputs, pair.Outputs);
                for (var i = NumberOfLayers() - 1; i >= 1; i--)
                {
                    var currentLayer = GetLayer(i);
                    for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                    {
                        totalGradients.IncrementThreshold(i, j, partialGradients.GetThreshold(i, j));
                        for (var k = 0; k < currentLayer.LowerLayer().NumberOfNeurons(); k++)
                        {
                            totalGradients.IncrementWeight(i, j, k, partialGradients.GetWeight(i, j, k));
                        }
                    }

                }
            }
        }

        private void Adaptation(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
        {
            double delta;
            var deltaGradients = new Gradients(this);
            var totalGradients = new Gradients(this);
            var partialGradients = new Gradients(this);

            Console.WriteLine("setting up random weights and thresholds ...");

            for (var i = NumberOfLayers() - 1; i >= 1; i--)
            {
                var currentLayer = GetLayer(i);
                for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                {
                    var currentNeuron = currentLayer.GetNeuron(j)
                        ;
                    currentNeuron.Threshold = 2 * Random() - 1;
                    for (var k = 0; k < currentNeuron.NumberOfInputs(); k++)
                    {
                        currentNeuron.GetInput(k).Weight = 2 * Random() - 1;
                    }
                }
            }

            var currK = 0;
            var currE = double.PositiveInfinity;
            Console.WriteLine("entering adaptation loop ... (maxK = " + maxK + ")");

            while (currK < maxK && currE > eps)
            {
                ComputeTotalGradient(totalGradients, partialGradients, trainingSet);
                for (var i = NumberOfLayers() - 1; i >= 1; i--)
                {
                    var currentLayer = GetLayer(i);
                    for (var j = 0; j < currentLayer.NumberOfNeurons(); j++)
                    {
                        var currentNeuron = currentLayer.GetNeuron(j);
                        delta = -lambda * totalGradients.GetThreshold(i, j)
                                + micro * deltaGradients.GetThreshold(i, j);
                        currentNeuron.Threshold += delta;
                        deltaGradients.SetThreshold(i, j, delta);
                    }

                    for (var k = 0; k < currentLayer.NumberOfNeurons(); k++)
                    {
                        var currentNeuron = currentLayer.GetNeuron(k);
                        for (var l = 0; l < currentNeuron.NumberOfInputs(); l++)
                        {
                            delta = -lambda * totalGradients.GetWeight(i, k, l) +
                                    micro * deltaGradients.GetWeight(i, k, l);
                            currentNeuron.GetInput(l).Weight += delta;
                            deltaGradients.SetWeight(i, k, l, delta);
                        }
                    }
                }

                currE = totalGradients.GetGradientAbs();
                currK++;
                if (currK % 25 == 0)
                {
                    Console.WriteLine("currK=" + currK + "   currE=" + currE);
                }
            }
        }

        private List<double> Activities(List<double> inputs)
        {
            for (var i = 0; i < NumberOfLayers(); i++)
            {
                for (var j = 0; j < GetLayer(i).NumberOfNeurons(); j++)
                {
                    var sum = GetLayer(i).GetNeuron(j).Threshold;
                    for (var k = 0; k < GetLayer(i).GetNeuron(j).NumberOfInputs(); k++)
                    {
                        if (i == 0)
                        {
                            sum += GetLayer(i).GetNeuron(j).GetInput(k).Weight *
                                   inputs[j];
                        }
                        else
                        {
                            sum += GetLayer(i).GetNeuron(j).GetInput(k).Weight *
                                   GetLayer(i - 1).GetNeuron(k).Output;
                        }
                    }
                    GetLayer(i).GetNeuron(j).Output = GainFunction(sum);
                }
            }

            var output = new List<double>();
            for (var i = 0; i < GetLayer(NumberOfLayers() - 1).NumberOfNeurons(); i++)
            {
                output.Add(GetLayer(NumberOfLayers() - 1).GetNeuron(i).Output);
            }

            return output;
        }
    }
}
