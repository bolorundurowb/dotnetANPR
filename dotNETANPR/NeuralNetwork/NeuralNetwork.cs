using System.Collections.Generic;

namespace dotNETANPR.NeuralNetwork
{
    public class NeuralNetwork
    {

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
            private List<NeuralInput> listInputs = new List<NeuralInput>();
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
                for (int i = 0; i < numberOfInputs; i++)
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
            private List<Neuron> listNeurons = new List<Neuron>();
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
                for (int i = 0; i < numberOfNeurons; i++)
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

            public Neuron GetNeuron()
            {
                return listNeurons[Index];
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
                for (int i = 0; i < NeuralNetwork.NumberOfLayers(); i++)
                {
                    thresholds.Add(new List<double>());
                    weights.Add(new List<List<double>>());
                    for (int j = 0; j < NeuralNetwork.GetLayer(i); j++)
                    {
                        thresholds[i].Add(0.0);
                        weights[i].Add(new List<double>());
                        for (int k = 0; k < NeuralNetwork.GetLayer(i).GetNeuron(j).NumberOfInputs(); k++)
                        {
                            weights[i][j].Add(0.0);
                        }
                    }
                }
            }

            private void ResetGradients()
            {
                for (int i = 0; i < NeuralNetwork.NumberOfLayers(); i++)
                {
                    for (int j = 0; j < NeuralNetwork.GetLayer(i).NumberOfNeurons(); j++)
                    {
                        SetThreshold(i, j, 0.0);
                        for (int k = 0; k < NeuralNetwork.GetLayer(i).GetNeuron(j).NumberOfInputs(); k++)
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

            public double GetWeight (int i, int j, int k)
            {
                return weights[i][j][k];
            }

            public void SetWeight (int i, int j, int k, double value)
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
                for (int i = 1; i < NeuralNetwork.NumberOfLayers(); i++)
                {
                    currE += VectorAbs(thresholds[i]);
                    currE += DoubleVectorAbs(weights[i]);
                }
                return currE;
            }
        }
    }
}
