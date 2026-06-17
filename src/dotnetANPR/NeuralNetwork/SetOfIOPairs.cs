using System.Collections.Generic;

namespace DotNetANPR.NeuralNetwork;

/// <summary>
/// Holds a collection of input/output vector pairs used for training a <see cref="NeuralNetwork"/>.
/// </summary>
public class SetOfIOPairs
{
    /// <summary>
    /// Gets the list of input/output pairs in this training set.
    /// </summary>
    public List<IOPair> Pairs { get; } = new();

    /// <summary>
    /// Creates a new <see cref="IOPair"/> from the given input and output vectors and adds it
    /// to the training set.
    /// </summary>
    /// <param name="inputs">The input vector.</param>
    /// <param name="outputs">The expected output vector.</param>
    public void AddIOPair(List<double> inputs, List<double> outputs) => AddIOPair(new IOPair(inputs, outputs));

    /// <summary>
    /// Adds an existing <see cref="IOPair"/> to the training set.
    /// </summary>
    /// <param name="pair">The pair to add.</param>
    public void AddIOPair(IOPair pair) => Pairs.Add(pair);

    /// <summary>
    /// Represents a single input/output vector pair for supervised learning.
    /// </summary>
    /// <param name="inputs">The input feature vector.</param>
    /// <param name="outputs">The expected output vector.</param>
    public class IOPair(List<double> inputs, List<double> outputs)
    {
        /// <summary>
        /// Gets the input feature vector.
        /// </summary>
        public List<double> Inputs { get; } = new(inputs);

        /// <summary>
        /// Gets the expected output vector.
        /// </summary>
        public List<double> Outputs { get; } = new(outputs);
    }
}
