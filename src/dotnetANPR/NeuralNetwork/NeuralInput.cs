namespace DotNetANPR.NeuralNetwork;

/// <summary>
/// Represents a single input connection to a <see cref="Neuron"/> in the neural network.
/// Each input has a weight that is adjusted during the learning process.
/// </summary>
/// <param name="weight">The initial weight of this input connection.</param>
/// <param name="neuron">The neuron this input belongs to.</param>
public class NeuralInput(double weight, Neuron neuron)
{
    /// <summary>
    /// Gets or sets the weight of this input connection.
    /// </summary>
    public double Weight { get; set; } = weight;

    /// <summary>
    /// Gets the neuron this input belongs to.
    /// </summary>
    public Neuron Neuron { get; } = neuron;

    /// <summary>
    /// Gets the positional index of this input within the parent neuron's input list.
    /// The index is determined at construction time.
    /// </summary>
    public int Index { get; } = neuron.Inputs.Count;
}
