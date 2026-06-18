using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
namespace DotNetANPR.NeuralNetwork;

/// <summary>
/// A multi-layer perceptron neural network with sigmoid activation.
/// Supports construction from explicit layer dimensions or from a saved XML file,
/// forward-pass testing, backpropagation learning with momentum, and XML serialization.
/// </summary>
public class NeuralNetwork
{
    private readonly Random _randomGenerator;

    /// <summary>
    /// Gets the ordered list of layers from bottom (input) to top (output).
    /// </summary>
    public List<NeuralLayer> Layers { get; } = new();

    /// <summary>
    /// Initializes a new <see cref="NeuralNetwork"/> with layers whose sizes are specified
    /// by the <paramref name="dimensions"/> list, ordered from input layer to output layer.
    /// </summary>
    /// <param name="dimensions">The number of neurons in each layer, bottom to top.</param>
    public NeuralNetwork(List<int> dimensions)
    {
        foreach (var dimension in dimensions)
            Layers.Add(new NeuralLayer(dimension, this));

        _randomGenerator = new Random();
    }

    /// <summary>
    /// Initializes a new <see cref="NeuralNetwork"/> by loading a previously saved topology
    /// from an XML file.
    /// </summary>
    /// <param name="path">The file path to the XML network definition.</param>
    public NeuralNetwork(string path) : this(File.OpenRead(path)) { }

    /// <summary>
    /// Initializes a new <see cref="NeuralNetwork"/> by loading a previously saved topology
    /// from an XML stream.
    /// </summary>
    /// <param name="stream">The stream containing the XML network definition.</param>
    public NeuralNetwork(Stream stream)
    {
        LoadFromXml(stream);
        _randomGenerator = new Random();
    }

    /// <summary>
    /// Performs a forward pass through the network with the given input vector and returns
    /// the output vector from the top layer.
    /// </summary>
    /// <param name="inputs">
    /// The input values. Must have the same count as the number of neurons in the input layer.
    /// </param>
    /// <returns>The output values from the top layer neurons.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the input vector size does not match the input layer size.
    /// </exception>
    public List<double> Test(List<double> inputs)
    {
        if (inputs.Count != Layers[0].Neurons.Count)
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + inputs.Count
                + " values into neural layer with " + Layers[0].Neurons.Count + " neurons.");

        return Activities(inputs);
    }

    /// <summary>
    /// Trains the network using backpropagation with momentum on the provided training set.
    /// </summary>
    /// <param name="trainingSet">The set of input/output pairs for training.</param>
    /// <param name="maxK">The maximum number of training iterations.</param>
    /// <param name="eps">The convergence threshold for gradient magnitude.</param>
    /// <param name="lambda">The learning rate.</param>
    /// <param name="micro">The momentum coefficient.</param>
    /// <exception cref="NullReferenceException">Thrown when the training set is empty.</exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when training pair dimensions do not match the network layer sizes.
    /// </exception>
    public void Learn(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
    {
        if (trainingSet.Pairs.Count == 0)
            throw new NullReferenceException(
                "[Error] NN-Learn: You are using an empty training set, neural network couldn't be trained.");

        if (trainingSet.Pairs[0].Inputs.Count != Layers[0].Neurons.Count)
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + trainingSet.Pairs[0].Inputs.Count
                + " values into neural layer with " + Layers[0].Neurons.Count
                + " neurons. Consider using another network, or another descriptors.");

        if (trainingSet.Pairs[0].Outputs.Count != Layers[Layers.Count - 1].Neurons.Count)
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + trainingSet.Pairs[0].Outputs.Count
                + " values into neural layer with " + Layers[Layers.Count - 1].Neurons.Count
                + " neurons. Consider using another network, or another descriptors.");

        Adaptation(trainingSet, maxK, eps, lambda, micro);
    }

    /// <summary>
    /// Saves the complete network topology (layers, neurons, thresholds, weights) to an XML file.
    /// </summary>
    /// <param name="fileName">The file path to save to.</param>
    public void SaveToXml(string fileName)
    {
        var root = new XElement("neuralNetwork",
            new XAttribute("dateOfExport", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

        var structure = new XElement("structure",
            new XAttribute("numberOfLayers", Layers.Count.ToString()));

        for (var il = 0; il < Layers.Count; il++)
        {
            var layerObj = Layers[il];
            var layerElement = new XElement("layer",
                new XAttribute("index", il.ToString()),
                new XAttribute("numberOfNeurons", layerObj.Neurons.Count.ToString()));

            for (var inIdx = 0; inIdx < layerObj.Neurons.Count; inIdx++)
            {
                var neuron = layerObj.Neurons[inIdx];
                var neuronElement = new XElement("neuron",
                    new XAttribute("index", inIdx.ToString()),
                    new XAttribute("NumberOfInputs", neuron.Inputs.Count.ToString()),
                    new XAttribute("threshold", neuron.Threshold.ToString(CultureInfo.InvariantCulture)));

                for (var ii = 0; ii < neuron.Inputs.Count; ii++)
                {
                    var input = neuron.Inputs[ii];
                    var inputElement = new XElement("input",
                        new XAttribute("index", ii.ToString()),
                        new XAttribute("weight", input.Weight.ToString(CultureInfo.InvariantCulture)));

                    neuronElement.Add(inputElement);
                }

                layerElement.Add(neuronElement);
            }

            structure.Add(layerElement);
        }

        root.Add(structure);
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        doc.Save(fileName);
    }

    #region Private Helpers

    private void LoadFromXml(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var nodeNeuralNetwork = doc.Root;
        if (nodeNeuralNetwork?.Name.LocalName != "neuralNetwork")
        {
            throw new FormatException("Parse error in XML file: root element is not 'neuralNetwork'.");
        }

        foreach (var nodeStructure in nodeNeuralNetwork.Elements("structure"))
        {
            foreach (var nodeLayer in nodeStructure.Elements("layer"))
            {
                var neuralLayer = new NeuralLayer(this);
                Layers.Add(neuralLayer);

                foreach (var nodeNeuron in nodeLayer.Elements("neuron"))
                {
                    var thresholdStr = nodeNeuron.Attribute("threshold")?.Value
                                       ?? throw new FormatException("Neuron missing 'threshold' attribute.");
                    var neuron = new Neuron(
                        double.Parse(thresholdStr, CultureInfo.InvariantCulture),
                        neuralLayer);
                    neuralLayer.Neurons.Add(neuron);

                    foreach (var nodeInput in nodeNeuron.Elements("input"))
                    {
                        var weightStr = nodeInput.Attribute("weight")?.Value
                                        ?? throw new FormatException("Input missing 'weight' attribute.");
                        var neuralInput = new NeuralInput(
                            double.Parse(weightStr, CultureInfo.InvariantCulture),
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
        for (var layerIndex = Layers.Count - 1; layerIndex >= 1; layerIndex--)
        {
            var currentLayer = Layers[layerIndex];
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
                            gradients.GetThreshold(layerIndex, neuronIndex) *
                            currentLayer.LowerLayer()!.Neurons[inputIndex].Output);
                }
            }
            else
            {
                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    double aux = 0;
                    var upperLayer = currentLayer.UpperLayer()!;
                    for (var ia = 0; ia < upperLayer.Neurons.Count; ia++)
                        aux += gradients.GetThreshold(layerIndex + 1, ia) *
                               upperLayer.Neurons[ia].Inputs[neuronIndex].Weight;

                    gradients.SetThreshold(layerIndex, neuronIndex,
                        currentLayer.Neurons[neuronIndex].Output *
                        (1 - currentLayer.Neurons[neuronIndex].Output) * aux);
                }

                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (var inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                        gradients.SetWeight(layerIndex, neuronIndex, inputIndex,
                            gradients.GetThreshold(layerIndex, neuronIndex) *
                            currentLayer.LowerLayer()!.Neurons[inputIndex].Output);
                }
            }
        }
    }

    private void ComputeTotalGradient(Gradients totalGradients, Gradients partialGradients,
        SetOfIOPairs trainingSet)
    {
        totalGradients.ResetGradients();
        foreach (var pair in trainingSet.Pairs)
        {
            ComputeGradient(partialGradients, pair.Inputs, pair.Outputs);
            for (var layerIndex = Layers.Count - 1; layerIndex >= 1; layerIndex--)
            {
                var currentLayer = Layers[layerIndex];
                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    totalGradients.IncrementThreshold(layerIndex, neuronIndex,
                        partialGradients.GetThreshold(layerIndex, neuronIndex));
                    for (var inputIndex = 0;
                         inputIndex < currentLayer.LowerLayer()!.Neurons.Count;
                         inputIndex++)
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

        // Initialize weights and thresholds to random values
        for (var layerIndex = Layers.Count - 1; layerIndex >= 1; layerIndex--)
        {
            var currentLayer = Layers[layerIndex];
            foreach (var currentNeuron in currentLayer.Neurons)
            {
                currentNeuron.Threshold = 2 * Random() - 1;
                foreach (var input in currentNeuron.Inputs)
                    input.Weight = 2 * Random() - 1;
            }
        }

        var curK = 0;
        var curE = double.PositiveInfinity;

        while (curK < maxK && curE > eps)
        {
            ComputeTotalGradient(totalGradients, partialGradients, trainingSet);
            for (var layerIndex = Layers.Count - 1; layerIndex >= 1; layerIndex--)
            {
                var currentLayer = Layers[layerIndex];
                double delta;

                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    delta = -lambda * totalGradients.GetThreshold(layerIndex, neuronIndex)
                            + micro * deltaGradients.GetThreshold(layerIndex, neuronIndex);
                    currentNeuron.Threshold += delta;
                    deltaGradients.SetThreshold(layerIndex, neuronIndex, delta);
                }

                for (var neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    var currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (var inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                    {
                        delta = -lambda * totalGradients.GetWeight(layerIndex, neuronIndex, inputIndex)
                                + micro * deltaGradients.GetWeight(layerIndex, neuronIndex, inputIndex);
                        currentNeuron.Inputs[inputIndex].Weight += delta;
                        deltaGradients.SetWeight(layerIndex, neuronIndex, inputIndex, delta);
                    }
                }
            }

            curE = totalGradients.GetGradientAbs();
            curK++;
        }
    }

    private List<double> Activities(List<double> inputs)
    {
        for (var layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
        {
            for (var neuronIndex = 0; neuronIndex < Layers[layerIndex].Neurons.Count; neuronIndex++)
            {
                var sum = Layers[layerIndex].Neurons[neuronIndex].Threshold;
                for (var inputIndex = 0;
                     inputIndex < Layers[layerIndex].Neurons[neuronIndex].Inputs.Count;
                     inputIndex++)
                {
                    if (layerIndex == 0)
                        sum += Layers[layerIndex].Neurons[neuronIndex].Inputs[inputIndex].Weight *
                               inputs[neuronIndex];
                    else
                        sum += Layers[layerIndex].Neurons[neuronIndex].Inputs[inputIndex].Weight *
                               Layers[layerIndex - 1].Neurons[inputIndex].Output;
                }

                Layers[layerIndex].Neurons[neuronIndex].Output = GainFunction(sum);
            }
        }

        var output = new List<double>();
        foreach (var neuron in Layers[Layers.Count - 1].Neurons)
            output.Add(neuron.Output);

        return output;
    }

    private static double GainFunction(double x) => 1.0 / (1.0 + Math.Exp(-x));

    #endregion
}
