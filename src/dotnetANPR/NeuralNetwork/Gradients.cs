using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.NeuralNetwork;

public class Gradients
{
    public List<List<double>> Thresholds { get; private set; }

    public List<List<List<double>>> Weights { get; private set; }

    public NeuralNetwork NeuralNetwork { get; private set; }

    public Gradients(NeuralNetwork network)
    {
        NeuralNetwork = network;
        Thresholds = [];
        Weights = [];

        InitGradients();
    }

    public void ResetGradients()
    {
        for (var layerIndex = 0; layerIndex < NeuralNetwork.Layers.Count; layerIndex++)
        {
            for (var neuronIndex = 0; neuronIndex < NeuralNetwork.Layers[layerIndex].Neurons.Count; neuronIndex++)
            {
                SetThreshold(layerIndex, neuronIndex, 0.0d);
                for (var inputIndex = 0;
                     inputIndex < NeuralNetwork.Layers[layerIndex].Neurons[neuronIndex]
                         .Inputs.Count;
                     inputIndex++)
                    SetWeight(layerIndex, neuronIndex, inputIndex, 0.0d);
            }
        }
    }

    public double GetThreshold(int layerIndex, int neuronIndex) => Thresholds[layerIndex][neuronIndex];

    public void SetThreshold(int layerIndex, int neuronIndex, double value) =>
        Thresholds[layerIndex][neuronIndex] = value;

    public void IncrementThreshold(int layerIndex, int neuronIndex, double value) =>
        SetThreshold(layerIndex, neuronIndex, GetThreshold(layerIndex, neuronIndex) + value);

    public double GetWeight(int layerIndex, int neuronIndex, int inputIndex) =>
        Weights[layerIndex][neuronIndex][inputIndex];

    public void SetWeight(int layerIndex, int neuronIndex, int inputIndex, double value) =>
        Weights[layerIndex][neuronIndex][inputIndex] = value;

    public void IncrementWeight(int layerIndex, int neuronIndex, int inputIndex, double value) => SetWeight(layerIndex,
        neuronIndex, inputIndex, GetWeight(layerIndex, neuronIndex, inputIndex) + value);

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

    private double DoubleListAbs(List<List<double>> doubleList)
    {
        var totalX = doubleList.Sum(vector => Math.Pow(ListAbs(vector), 2));
        return Math.Sqrt(totalX);
    }

    private double ListAbs(List<double> list)
    {
        var totalX = list.Sum(x => Math.Pow(x, 2));
        return Math.Sqrt(totalX);
    }

    private void InitGradients()
    {
        for (var layerIndex = 0; layerIndex < NeuralNetwork.Layers.Count; layerIndex++)
        {
            Thresholds.Add([]);
            Weights.Add([]);
            for (var neuronIndex = 0; neuronIndex < NeuralNetwork.Layers[layerIndex].Neurons.Count; neuronIndex++)
            {
                Thresholds[layerIndex].Add(0.0);
                Weights[layerIndex].Add([]);
                for (var inputIndex = 0;
                     inputIndex < NeuralNetwork.Layers[layerIndex].Neurons[neuronIndex]
                         .Inputs.Count;
                     inputIndex++)
                    Weights[layerIndex][neuronIndex].Add(0.0);
            }
        }
    }

    #endregion
}
