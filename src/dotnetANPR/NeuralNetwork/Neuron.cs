using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.NeuralNetwork;

public class Neuron
{
    public double Threshold { get; set; }
    
    public double Output { get; set; }

    public NeuralLayer NeuralLayer { get; }

    public int Index { get; }

    public List<NeuralInput> Inputs { get; }

    public Neuron(double threshold, NeuralLayer neuralLayer)
    {
        Threshold = threshold;
        NeuralLayer = neuralLayer;
        Index = neuralLayer.Neurons.Count;
        Inputs = new();
    }

    public Neuron(int numOfInputs, double threshold, NeuralLayer neuralLayer) : this(threshold, neuralLayer) =>
        Inputs = Enumerable.Repeat(new NeuralInput(1d, this), numOfInputs).ToList();
}
