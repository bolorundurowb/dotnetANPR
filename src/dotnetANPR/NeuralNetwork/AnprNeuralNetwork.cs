using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DotNetANPR.NeuralNetwork;

public class AnprNeuralNetwork
{
    private readonly NeuralNetworkModel _model;
    public Dictionary<int, char> Map { get; }

    public AnprNeuralNetwork(string jsonPath, string mapPath)
    {
        _model = LoadNetwork(jsonPath);
        Map = LoadMap(mapPath);
    }

    private NeuralNetworkModel LoadNetwork(string path)
    {
        var jsonString = File.ReadAllText(path);
        return JsonSerializer.Deserialize<NeuralNetworkModel>(jsonString);
    }

    private Dictionary<int, char> LoadMap(string path)
    {
        var map = new Dictionary<int, char>();
        var index = 0;
        // The Java version uses the alphabet directory, sorted, to create the map
        var files = Directory.GetFiles(path, "*.jpg")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(f => f);

        foreach (var file in files)
        {
            map[index++] = file[0];
        }
        return map;
    }

    public double[] Compute(float[] input)
    {
        // 1. Set input layer outputs
        var inputLayer = _model.Layers[0];
        if (input.Length != inputLayer.Neurons.Count)
            throw new ArgumentException("Input vector size does not match network input layer.");

        for (var i = 0; i < inputLayer.Neurons.Count; i++)
        {
            inputLayer.Neurons[i].Output = input[i];
        }

        // 2. Feed-forward through hidden/output layers
        for (var i = 1; i < _model.Layers.Count; i++)
        {
            var layer = _model.Layers[i];
            var prevLayer = _model.Layers[i - 1];

            foreach (var neuron in layer.Neurons)
            {
                double sum = 0;
                // Get weights from the *previous* layer's neurons
                for (var j = 0; j < prevLayer.Neurons.Count; j++)
                {
                    sum += prevLayer.Neurons[j].Output * prevLayer.Neurons[j].Weights[i - 1];
                }
                neuron.Output = Sigmoid(sum);
            }
        }

        // 3. Return output of the last layer
        return _model.Layers.Last().Neurons.Select(n => n.Output).ToArray();
    }

    private double Sigmoid(double x)
    {
        return 1.0 / (1.0 + Math.Exp(-x));
    }
}