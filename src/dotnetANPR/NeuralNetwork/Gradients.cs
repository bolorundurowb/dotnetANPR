using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.NeuralNetwork;

/// <summary>
/// Stores gradient arrays for backpropagation learning in a <see cref="NeuralNetwork"/>.
/// Maintains per-layer threshold gradients and per-input weight gradients that are
/// accumulated during training.
/// </summary>
public class Gradients
{
    /// <summary>
    /// Gets the threshold gradients, indexed by [layerIndex][neuronIndex].
    /// </summary>
    public List<List<double>> Thresholds { get; private set; }

    /// <summary>
    /// Gets the weight gradients, indexed by [layerIndex][neuronIndex][inputIndex].
    /// </summary>
    public List<List<List<double>>> Weights { get; private set; }

    /// <summary>
    /// Gets the parent neural network whose structure these gradients mirror.
    /// </summary>
    public NeuralNetwork NeuralNetwork { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="Gradients"/> instance that mirrors the structure of
    /// the specified network, with all values set to zero.
    /// </summary>
    /// <param name="network">The neural network whose topology to mirror.</param>
    public Gradients(NeuralNetwork network)
    {
        NeuralNetwork = network;
        Thresholds = new List<List<double>>();
        Weights = new List<List<List<double>>>();

        InitGradients();
    }

    /// <summary>
    /// Resets all threshold and weight gradient values to zero.
    /// </summary>
    public void ResetGradients()
    {
        for (var layerIndex = 0; layerIndex < NeuralNetwork.Layers.Count; layerIndex++)
        {
            for (var neuronIndex = 0; neuronIndex < NeuralNetwork.Layers[layerIndex].Neurons.Count; neuronIndex++)
            {
                SetThreshold(layerIndex, neuronIndex, 0.0);
                for (var inputIndex = 0;
                     inputIndex < NeuralNetwork.Layers[layerIndex].Neurons[neuronIndex].Inputs.Count;
                     inputIndex++)
                    SetWeight(layerIndex, neuronIndex, inputIndex, 0.0);
            }
        }
    }

    /// <summary>
    /// Gets the threshold gradient for the specified neuron.
    /// </summary>
    /// <param name="layerIndex">The layer index.</param>
    /// <param name="neuronIndex">The neuron index within the layer.</param>
    /// <returns>The threshold gradient value.</returns>
    public double GetThreshold(int layerIndex, int neuronIndex) => Thresholds[layerIndex][neuronIndex];

    /// <summary>
    /// Sets the threshold gradient for the specified neuron.
    /// </summary>
    /// <param name="layerIndex">The layer index.</param>
    /// <param name="neuronIndex">The neuron index within the layer.</param>
    /// <param name="value">The gradient value to set.</param>
    public void SetThreshold(int layerIndex, int neuronIndex, double value) =>
        Thresholds[layerIndex][neuronIndex] = value;

    /// <summary>
    /// Adds the specified value to the existing threshold gradient for the specified neuron.
    /// </summary>
    /// <param name="layerIndex">The layer index.</param>
    /// <param name="neuronIndex">The neuron index within the layer.</param>
    /// <param name="value">The value to add.</param>
    public void IncrementThreshold(int layerIndex, int neuronIndex, double value) =>
        SetThreshold(layerIndex, neuronIndex, GetThreshold(layerIndex, neuronIndex) + value);

    /// <summary>
    /// Gets the weight gradient for the specified input connection.
    /// </summary>
    /// <param name="layerIndex">The layer index.</param>
    /// <param name="neuronIndex">The neuron index within the layer.</param>
    /// <param name="inputIndex">The input index within the neuron.</param>
    /// <returns>The weight gradient value.</returns>
    public double GetWeight(int layerIndex, int neuronIndex, int inputIndex) =>
        Weights[layerIndex][neuronIndex][inputIndex];

    /// <summary>
    /// Sets the weight gradient for the specified input connection.
    /// </summary>
    /// <param name="layerIndex">The layer index.</param>
    /// <param name="neuronIndex">The neuron index within the layer.</param>
    /// <param name="inputIndex">The input index within the neuron.</param>
    /// <param name="value">The gradient value to set.</param>
    public void SetWeight(int layerIndex, int neuronIndex, int inputIndex, double value) =>
        Weights[layerIndex][neuronIndex][inputIndex] = value;

    /// <summary>
    /// Adds the specified value to the existing weight gradient for the specified input connection.
    /// </summary>
    /// <param name="layerIndex">The layer index.</param>
    /// <param name="neuronIndex">The neuron index within the layer.</param>
    /// <param name="inputIndex">The input index within the neuron.</param>
    /// <param name="value">The value to add.</param>
    public void IncrementWeight(int layerIndex, int neuronIndex, int inputIndex, double value) =>
        SetWeight(layerIndex, neuronIndex, inputIndex, GetWeight(layerIndex, neuronIndex, inputIndex) + value);

    /// <summary>
    /// Computes the absolute value (L2 norm) of the entire gradient vector,
    /// used to check convergence during training.
    /// </summary>
    /// <returns>The overall gradient magnitude.</returns>
    public double GetGradientAbs()
    {
        double currE = 0;
        for (var layerIndex = 1; layerIndex < NeuralNetwork.Layers.Count; layerIndex++)
        {
            currE += ListAbs(Thresholds[layerIndex]);
            currE += DoubleListAbs(Weights[layerIndex]);
        }

        return currE;
    }

    #region Private Helpers

    private static double DoubleListAbs(List<List<double>> doubleList)
    {
        var totalX = doubleList.Sum(vector => Math.Pow(ListAbs(vector), 2));
        return Math.Sqrt(totalX);
    }

    private static double ListAbs(List<double> list)
    {
        var totalX = list.Sum(x => Math.Pow(x, 2));
        return Math.Sqrt(totalX);
    }

    private void InitGradients()
    {
        for (var layerIndex = 0; layerIndex < NeuralNetwork.Layers.Count; layerIndex++)
        {
            Thresholds.Add(new List<double>());
            Weights.Add(new List<List<double>>());
            for (var neuronIndex = 0; neuronIndex < NeuralNetwork.Layers[layerIndex].Neurons.Count; neuronIndex++)
            {
                Thresholds[layerIndex].Add(0.0);
                Weights[layerIndex].Add(new List<double>());
                for (var inputIndex = 0;
                     inputIndex < NeuralNetwork.Layers[layerIndex].Neurons[neuronIndex].Inputs.Count;
                     inputIndex++)
                    Weights[layerIndex][neuronIndex].Add(0.0);
            }
        }
    }

    #endregion
}
