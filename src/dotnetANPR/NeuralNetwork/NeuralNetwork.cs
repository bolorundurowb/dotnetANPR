using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using DotNetANPR.Utilities;
using Microsoft.Extensions.Logging;

namespace DotNetANPR.NeuralNetwork;

public class NeuralNetwork
{
    private static readonly ILogger<NeuralNetwork> Logger = Logging.GetLogger<NeuralNetwork>();

    private readonly Random _randomGenerator;

    public List<NeuralLayer> Layers { get; } = new();

    public NeuralNetwork(List<int> dimensions)
    {
        foreach (var dimension in dimensions)
            Layers.Add(new NeuralLayer(dimension, this));

        _randomGenerator = new Random();
        Logger.LogInformation("Created neural network with " + dimensions.Count + " layers");
    }

    public NeuralNetwork(string path)
    {
        LoadFromXml(path);
        _randomGenerator = new Random();
    }

    public List<double> Test(List<double> inputs)
    {
        if (inputs.Count != Layers[0].Neurons.Count)
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + inputs.Count
                                                                       + " values into neural layer with " +
                                                                       Layers[0].Neurons.Count + " neurons. ");

        return Activities(inputs);
    }

    public void Learn(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
    {
        if (trainingSet.Pairs.Count == 0)
            throw new NullReferenceException(
                "[Error] NN-Learn: You are using an empty training set, neural network couldn't be trained.");

        if (trainingSet.Pairs[0].Inputs.Count != Layers[0].Neurons.Count)
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + trainingSet.Pairs[0].Inputs
                    .Count + " values into neural layer with " + Layers[0].Neurons.Count
                + " neurons. Consider using another network, or another " + "descriptors.");

        if (trainingSet.Pairs[0].Outputs.Count != GetLayer(Layers.Count - 1)
                .Neurons.Count)
            throw new IndexOutOfRangeException(
                "[Error] NN-Test:  You are trying to pass vector with " + trainingSet.Pairs[0].Inputs
                    .Count + " values into neural layer with " + Layers[0].Neurons.Count
                + " neurons. Consider using another network, or another " + "descriptors.");

        Adaptation(trainingSet, maxK, eps, lambda, micro);
    }

    public void SaveToXml(string fileName)
    {
        Logger.LogInformation("Saving network topology to file " + fileName);

        var doc = new XmlDocument();

        var root = doc.CreateElement("neuralNetwork");
        root.SetAttribute("dateOfExport", DateTime.Now.ToString());
        var layers = doc.CreateElement("structure");
        layers.SetAttribute("numberOfLayers", Layers.Count.ToString());

        for (var il = 0; il < Layers.Count; il++)
        {
            var layerObj = Layers[il];
            var layer = doc.CreateElement("layer");
            layer.SetAttribute("index", il.ToString());
            layer.SetAttribute("numberOfNeurons", layerObj.Neurons.Count.ToString());

            for (var inIdx = 0; inIdx < layerObj.Neurons.Count; inIdx++)
            {
                var neuron = layerObj.Neurons[inIdx];
                var neuronElement = doc.CreateElement("neuron");
                neuronElement.SetAttribute("index", inIdx.ToString());
                neuronElement.SetAttribute("NumberOfInputs", neuron.Inputs.Count.ToString());
                neuronElement.SetAttribute("threshold", neuron.Threshold.ToString());

                for (var ii = 0; ii < neuron.Inputs.Count; ii++)
                {
                    var input = neuron.Inputs[ii];
                    var inputElement = doc.CreateElement("input");
                    inputElement.SetAttribute("index", ii.ToString());
                    inputElement.SetAttribute("weight", input.Weight.ToString());
                    neuronElement.AppendChild(inputElement);
                }

                layer.AppendChild(neuronElement);
            }

            layers.AppendChild(layer);
        }

        root.AppendChild(layers);
        doc.AppendChild(root);

        using var fs = new FileStream(fileName, FileMode.Create);
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = System.Text.Encoding.GetEncoding("iso-8859-2")
        };
        using var writer = XmlWriter.Create(fs, settings);
        doc.Save(writer);
    }

    public void PrintNeuralNetwork()
    {
        for (var layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
        {
            Console.WriteLine("Layer " + layerIndex);
            for (var neuronIndex = 0; neuronIndex < GetLayer(layerIndex).Neurons.Count; neuronIndex++)
            {
                Console.Write("      Neuron " + neuronIndex + " (threshold=" + GetLayer(layerIndex)
                    .Neurons[neuronIndex].Threshold + ") : ");
                foreach (var t in GetLayer(layerIndex).Neurons[neuronIndex].Inputs)
                    Console.Write(t.Weight + " ");

                Console.WriteLine();
            }
        }
    }


    #region Private Helpers

    private void LoadFromXml(string path)
    {
        Logger.LogDebug("Loading network topology from InputStream");

        var doc = new XmlDocument();

        try
        {
            doc.Load(path);

            if (doc == null)
                throw new NullReferenceException();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }

        var nodeNeuralNetwork = doc?.DocumentElement;

        if (nodeNeuralNetwork?.Name != "neuralNetwork")
        {
            Logger.LogError("Parse error in XML file, neural network couldn't be loaded.");
            return;
        }

        var nodeNeuralNetworkContent = nodeNeuralNetwork.ChildNodes;

        foreach (XmlNode nodeStructure in nodeNeuralNetworkContent)
            if (nodeStructure.Name == "structure")
            {
                var nodeStructureContent = nodeStructure.ChildNodes;

                foreach (XmlNode nodeLayer in nodeStructureContent)
                    if (nodeLayer.Name == "layer")
                    {
                        var neuralLayer = new NeuralLayer(this);
                        Layers.Add(neuralLayer);

                        var nodeLayerContent = nodeLayer.ChildNodes;

                        foreach (XmlNode nodeNeuron in nodeLayerContent)
                            if (nodeNeuron.Name == "neuron")
                            {
                                var neuron = new Neuron(
                                    double.Parse(((XmlElement)nodeNeuron).GetAttribute("threshold")),
                                    neuralLayer);
                                neuralLayer.Neurons.Add(neuron);

                                var nodeNeuronContent = nodeNeuron.ChildNodes;

                                foreach (XmlNode nodeNeuralInput in nodeNeuronContent)
                                    if (nodeNeuralInput.Name == "input")
                                    {
                                        Logger.LogDebug("neuron at STR: {0} LAY: {1} NEU: {2} INP: {3}",
                                            Layers.Count - 1, neuralLayer.Neurons.Count - 1,
                                            neuralLayer.Neurons.IndexOf(neuron),
                                            neuron.Inputs.Count);

                                        var neuralInput = new NeuralInput(
                                            double.Parse(((XmlElement)nodeNeuralInput).GetAttribute("weight")),
                                            neuron);
                                        neuron.Inputs.Add(neuralInput);
                                    }
                            }
                    }
            }
    }

    private double Random() => _randomGenerator.NextDouble();

    private void ComputeGradient(Gradients gradients, List<double> inputs, List<double> requiredOutputs)
    {
        Activities(inputs);
        for (var layerIndex = Layers.Count - 1;
             layerIndex >= 1;
             layerIndex--)
        {
            var currentLayer = GetLayer(layerIndex);
            if (currentLayer.IsTopLayer)
            {
                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    gradients.SetThreshold(layerIndex, neuronIndex,
                        currentNeuron.Output * (1 - currentNeuron.Output) *
                        (currentNeuron.Output - requiredOutputs[neuronIndex]));
                }

                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (var inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                        gradients.SetWeight(layerIndex, neuronIndex, inputIndex,
                            gradients.GetThreshold(layerIndex, neuronIndex) * currentLayer.LowerLayer()!
                                .Neurons[inputIndex].Output);
                }
            }
            else
            {
                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var aux = currentLayer.UpperLayer()!.Neurons
                        .Select((t, axonIndex) =>
                            gradients.GetThreshold(layerIndex + 1, axonIndex) * t.Inputs[neuronIndex].Weight)
                        .Sum();

                    gradients.SetThreshold(layerIndex, neuronIndex,
                        currentLayer.Neurons[neuronIndex].Output * (1 - currentLayer
                            .Neurons[neuronIndex].Output) * aux);
                }

                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (var inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                        gradients.SetWeight(layerIndex, neuronIndex, inputIndex,
                            gradients.GetThreshold(layerIndex, neuronIndex) * currentLayer.LowerLayer()!
                                .Neurons[inputIndex].Output);
                }
            }
        }
    }

    private void ComputeTotalGradient(Gradients totalGradients, Gradients partialGradients, SetOfIOPairs trainingSet)
    {
        totalGradients.ResetGradients();
        foreach (var pair in trainingSet.Pairs)
        {
            ComputeGradient(partialGradients, pair.Inputs, pair.Outputs);
            for (var layerIndex = Layers.Count - 1;
                 layerIndex >= 1;
                 layerIndex--)
            {
                var currentLayer = GetLayer(layerIndex);
                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    totalGradients.IncrementThreshold(layerIndex, neuronIndex,
                        partialGradients.GetThreshold(layerIndex, neuronIndex));
                    for (var inputIndex = 0; inputIndex < currentLayer.LowerLayer()!.Neurons.Count; inputIndex++)
                        totalGradients.IncrementWeight(layerIndex, neuronIndex, inputIndex,
                            partialGradients.GetWeight(layerIndex, neuronIndex, inputIndex));
                }
            }
        }
    }

    private void Adaptation(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
    {
        var deltaGradients = new Gradients(this);
        var totalGradients = new Gradients(this);
        var partialGradients = new Gradients(this);
        Logger.LogDebug("Setting up random weights and thresholds ...");

        for (var layerIndex = Layers.Count - 1;
             layerIndex >= 1;
             layerIndex--)
        {
            var currentLayer = GetLayer(layerIndex);
            foreach (var currentNeuron in currentLayer.Neurons)
            {
                currentNeuron.Threshold = 2 * Random() - 1;
                foreach (var t in currentNeuron.Inputs)
                    t.Weight = 2 * Random() - 1;
            }
        }

        var curK = 0;
        var curE = double.PositiveInfinity;
        Logger.LogDebug("Entering adaptation loop ... (maxK = " + maxK + ")");

        while (curK < maxK && curE > eps)
        {
            ComputeTotalGradient(totalGradients, partialGradients, trainingSet);
            for (var layerIndex = Layers.Count - 1;
                 layerIndex >= 1;
                 layerIndex--)
            {
                // top down all layers except last one
                var currentLayer = GetLayer(layerIndex);
                double delta;
                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    delta = -lambda * totalGradients.GetThreshold(layerIndex, neuronIndex) + micro * deltaGradients
                        .GetThreshold(layerIndex, neuronIndex);
                    currentNeuron.Threshold += delta;
                    deltaGradients.SetThreshold(layerIndex, neuronIndex, delta);
                }

                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (var inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                    {
                        delta = -lambda * totalGradients.GetWeight(layerIndex, neuronIndex, inputIndex) + micro
                            * deltaGradients.GetWeight(layerIndex, neuronIndex, inputIndex);
                        currentNeuron.Inputs[inputIndex].Weight += delta;
                        deltaGradients.SetWeight(layerIndex, neuronIndex, inputIndex, delta);
                    }
                }
            }

            curE = totalGradients.GetGradientAbs();
            curK++;

            if (curK % 25 == 0)
                Logger.LogDebug("curK=" + curK + ", curE=" + curE);
        }
    }

    private List<double> Activities(List<double> inputs)
    {
        for (var layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
            for (var neuronIndex = 0; neuronIndex < GetLayer(layerIndex).Neurons.Count; neuronIndex++)
            {
                var sum = GetLayer(layerIndex).Neurons[neuronIndex].Threshold; // sum <- threshold
                for (var inputIndex = 0;
                     inputIndex < GetLayer(layerIndex).Neurons[neuronIndex].Inputs.Count;
                     inputIndex++)
                    if (layerIndex == 0)
                        sum += GetLayer(layerIndex).Neurons[neuronIndex].Inputs[inputIndex].Weight *
                               inputs[neuronIndex];
                    else
                        sum += GetLayer(layerIndex).Neurons[neuronIndex].Inputs[inputIndex]
                            .Weight * GetLayer(layerIndex - 1).Neurons[inputIndex].Output;

                GetLayer(layerIndex).Neurons[neuronIndex].Output = GainFunction(sum);
            }

        List<double> output = new();
        foreach (var t in GetLayer(Layers.Count - 1).Neurons)
            output.Add(t.Output);

        return output;
    }

    private double GainFunction(double x) => 1 / (1 + Math.Exp(-x));

    private NeuralLayer GetLayer(int index) => Layers[index];

    #endregion
}
