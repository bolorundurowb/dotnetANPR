using System.Collections.Generic;
using System.Linq;

namespace dotnetANPR.NeuralNetwork;

/// <summary>
/// Represents a single neuron in a <see cref="NeuralLayer"/> with a threshold, weighted inputs, and an activation output.
/// </summary>
internal class Neuron
{
    /// <summary>The activation threshold (bias).</summary>
    public double Threshold { get; set; }

    /// <summary>The output value after activation.</summary>
    public double Output { get; set; }

    /// <summary>The layer this neuron belongs to.</summary>
    public NeuralLayer NeuralLayer { get; }

    /// <summary>The zero-based index of this neuron within its layer.</summary>
    public int Index { get; }

    /// <summary>The weighted inputs feeding into this neuron.</summary>
    public List<NeuralInput> Inputs { get; }

    /// <summary>
    /// Creates a neuron with the specified threshold.
    /// </summary>
    public Neuron(double threshold, NeuralLayer neuralLayer)
    {
        Threshold = threshold;
        NeuralLayer = neuralLayer;
        Index = neuralLayer.Neurons.Count;
        Inputs = new();
    }

    /// <summary>
    /// Creates a neuron with a specified number of inputs (for initialising network topology).
    /// </summary>
    public Neuron(int numOfInputs, double threshold, NeuralLayer neuralLayer) : this(threshold, neuralLayer) =>
        Inputs = Enumerable.Repeat(new NeuralInput(1d, this), numOfInputs).ToList();
}
