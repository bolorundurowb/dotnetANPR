using System.Collections.Generic;

namespace DotNetANPR.NeuralNetwork;

/// <summary>
/// Represents a single layer of neurons within a <see cref="NeuralNetwork"/>.
/// Provides navigation to adjacent layers and manages the collection of neurons.
/// </summary>
public class NeuralLayer
{
    /// <summary>
    /// Gets the positional index of this layer within the parent network.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the parent <see cref="NeuralNetwork"/> this layer belongs to.
    /// </summary>
    public NeuralNetwork NeuralNetwork { get; }

    /// <summary>
    /// Gets the list of neurons in this layer.
    /// </summary>
    public List<Neuron> Neurons { get; } = new();

    /// <summary>
    /// Gets a value indicating whether this is the topmost (output) layer.
    /// </summary>
    public bool IsTopLayer => Index == NeuralNetwork.Layers.Count - 1;

    /// <summary>
    /// Gets a value indicating whether this is the bottommost (input) layer.
    /// </summary>
    public bool IsBottomLayer => Index == 0;

    /// <summary>
    /// Initializes a new empty <see cref="NeuralLayer"/> and assigns it the next available index
    /// in the parent network.
    /// </summary>
    /// <param name="neuralNetwork">The parent neural network.</param>
    public NeuralLayer(NeuralNetwork neuralNetwork)
    {
        NeuralNetwork = neuralNetwork;
        Index = NeuralNetwork.Layers.Count;
    }

    /// <summary>
    /// Initializes a new <see cref="NeuralLayer"/> with the specified number of neurons.
    /// Neurons in the bottom layer (index 0) get one input each; neurons in higher layers
    /// get as many inputs as there are neurons in the layer below.
    /// </summary>
    /// <param name="numOfNeurons">The number of neurons to create in this layer.</param>
    /// <param name="neuralNetwork">The parent neural network.</param>
    public NeuralLayer(int numOfNeurons, NeuralNetwork neuralNetwork) : this(neuralNetwork)
    {
        for (var i = 0; i < numOfNeurons; i++)
            Neurons.Add(Index == 0
                ? new Neuron(1, 0, this)
                : new Neuron(NeuralNetwork.Layers[Index - 1].Neurons.Count, 0, this));
    }

    /// <summary>
    /// Gets the layer directly above this one, or <c>null</c> if this is the top layer.
    /// </summary>
    /// <returns>The upper <see cref="NeuralLayer"/>, or <c>null</c>.</returns>
    public NeuralLayer? UpperLayer() => IsTopLayer ? null : NeuralNetwork.Layers[Index + 1];

    /// <summary>
    /// Gets the layer directly below this one, or <c>null</c> if this is the bottom layer.
    /// </summary>
    /// <returns>The lower <see cref="NeuralLayer"/>, or <c>null</c>.</returns>
    public NeuralLayer? LowerLayer() => IsBottomLayer ? null : NeuralNetwork.Layers[Index - 1];
}
