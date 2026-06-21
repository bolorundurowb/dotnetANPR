using System.Collections.Generic;

namespace dotnetANPR.NeuralNetwork;

/// <summary>
/// A training set containing input-output pairs for neural network learning.
/// </summary>
internal class SetOfIOPairs
{
    /// <summary>The list of input-output pairs in this training set.</summary>
    public List<IOPair> Pairs { get; } = new();

    /// <summary>
    /// Adds a pair using separate input and output vectors.
    /// </summary>
    public void AddIOPair(List<double> inputs, List<double> outputs) => AddIOPair(new IOPair(inputs, outputs));

    /// <summary>
    /// Adds a pre-constructed input-output pair.
    /// </summary>
    public void AddIOPair(IOPair pair) => Pairs.Add(pair);

    /// <summary>
    /// A single training example pairing an input vector with its expected output vector.
    /// </summary>
    internal class IOPair(List<double> inputs, List<double> outputs)
    {
        /// <summary>The input feature vector.</summary>
        public List<double> Inputs { get; } = inputs;

        /// <summary>The expected output vector.</summary>
        public List<double> Outputs { get; } = outputs;
    }
}
