using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.NeuralNetwork;

/// <summary>
/// Represents a single neuron within a <see cref="NeuralLayer"/>.
/// A neuron holds a threshold (bias), computes an output value during the forward pass,
/// and maintains a list of weighted <see cref="NeuralInput"/> connections from the layer below.
/// </summary>
public class Neuron
{
    /// <summary>
    /// Gets or sets the threshold (bias) value for this neuron.
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Gets or sets the computed output value of this neuron after the forward pass.
    /// </summary>
    public double Output { get; set; }

    /// <summary>
    /// Gets the <see cref="NeuralLayer"/> this neuron belongs to.
    /// </summary>
    public NeuralLayer NeuralLayer { get; }

    /// <summary>
    /// Gets the positional index of this neuron within its layer.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the list of weighted input connections to this neuron.
    /// </summary>
    public List<NeuralInput> Inputs { get; }

    /// <summary>
    /// Initializes a new <see cref="Neuron"/> with the specified threshold and parent layer.
    /// The neuron starts with no input connections.
    /// </summary>
    /// <param name="threshold">The initial threshold (bias) value.</param>
    /// <param name="neuralLayer">The layer this neuron belongs to.</param>
    public Neuron(double threshold, NeuralLayer neuralLayer)
    {
        Threshold = threshold;
        NeuralLayer = neuralLayer;
        Index = neuralLayer.Neurons.Count;
        Inputs = new List<NeuralInput>();
    }

    /// <summary>
    /// Initializes a new <see cref="Neuron"/> with the specified number of input connections,
    /// each with an initial weight of 1.0.
    /// </summary>
    /// <param name="numOfInputs">The number of input connections to create.</param>
    /// <param name="threshold">The initial threshold (bias) value.</param>
    /// <param name="neuralLayer">The layer this neuron belongs to.</param>
    public Neuron(int numOfInputs, double threshold, NeuralLayer neuralLayer) : this(threshold, neuralLayer)
    {
        for (var i = 0; i < numOfInputs; i++)
            Inputs.Add(new NeuralInput(1.0, this));
    }
}
