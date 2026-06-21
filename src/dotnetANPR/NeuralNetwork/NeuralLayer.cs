using System.Collections.Generic;

namespace dotnetANPR.NeuralNetwork;

/// <summary>
/// Represents a layer of <see cref="Neuron"/> objects within a <see cref="NeuralNetwork"/>.
/// </summary>
public class NeuralLayer
{
    /// <summary>The zero-based index of this layer within the network.</summary>
    public int Index { get; }

    /// <summary>The network this layer belongs to.</summary>
    public NeuralNetwork NeuralNetwork { get; }

    /// <summary>The neurons in this layer.</summary>
    public List<Neuron> Neurons { get; } = new();

    /// <summary>Whether this is the output (top) layer.</summary>
    public bool IsTopLayer => Index == NeuralNetwork.Layers.Count - 1;

    /// <summary>Whether this is the input (bottom) layer.</summary>
    public bool IsBottomLayer => Index == 0;

    /// <summary>
    /// Creates an empty layer attached to the given network.
    /// </summary>
    public NeuralLayer(NeuralNetwork neuralNetwork)
    {
        NeuralNetwork = neuralNetwork;
        Index = NeuralNetwork.Layers.Count;
    }

    /// <summary>
    /// Creates a layer with the specified number of neurons, each properly connected to the previous layer.
    /// </summary>
    public NeuralLayer(int numOfNeurons, NeuralNetwork neuralNetwork) : this(neuralNetwork)
    {
        for (var i = 0; i < numOfNeurons; i++)
            Neurons.Add(Index == 0
                ? new Neuron(1, 0, this)
                : new Neuron(NeuralNetwork.Layers[Index - 1].Neurons.Count, 0, this));
    }

    /// <summary>Returns the layer above this one, or <c>null</c> if this is the top layer.</summary>
    public NeuralLayer? UpperLayer() => IsTopLayer ? null : NeuralNetwork.Layers[Index + 1];

    /// <summary>Returns the layer below this one, or <c>null</c> if this is the bottom layer.</summary>
    public NeuralLayer? LowerLayer() => IsBottomLayer ? null : NeuralNetwork.Layers[Index - 1];
}
