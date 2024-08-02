using System.Collections.Generic;

namespace DotNetANPR.NeuralNetwork;

public class NeuralLayer
{
    public int Index { get; }

    public NeuralNetwork NeuralNetwork { get; }

    public List<Neuron> Neurons { get; } = new();

    public bool IsTopLayer => Index == NeuralNetwork.Layers.Count - 1;

    public bool IsBottomLayer => Index == 0;

    public NeuralLayer(NeuralNetwork neuralNetwork)
    {
        NeuralNetwork = neuralNetwork;
        Index = NeuralNetwork.Layers.Count;
    }

    public NeuralLayer(int numOfNeurons, NeuralNetwork neuralNetwork) : this(neuralNetwork)
    {
        for (var i = 0; i < numOfNeurons; i++)
            Neurons.Add(Index == 0
                ? new Neuron(1, 0, this)
                : new Neuron(NeuralNetwork.Layers[Index - 1].Neurons.Count, 0, this));
    }

    public NeuralLayer? UpperLayer() => IsTopLayer ? null : NeuralNetwork.Layers[Index + 1];

    public NeuralLayer? LowerLayer() => IsBottomLayer ? null : NeuralNetwork.Layers[Index - 1];
}
