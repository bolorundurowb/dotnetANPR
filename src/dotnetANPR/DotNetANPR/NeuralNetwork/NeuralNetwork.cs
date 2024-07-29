using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace DotNetANPR.NeuralNetwork;

public class NeuralNetwork
{
    private static readonly ILogger<NeuralNetwork> logger =
        LoggerFactory.Create(_ => { }).CreateLogger<NeuralNetwork>();

    private Random randomGenerator;

    public List<NeuralLayer> Layers { get; } = new();

    public NeuralNetwork(List<int> dimensions)
    {
        foreach (int dimension in dimensions)
        {
            Layers.Add(new NeuralLayer(dimension, this));
        }

        randomGenerator = new Random();
        logger.LogInformation("Created neural network with " + dimensions.Count + " layers");
    }

    public NeuralNetwork(string path)
    {
        LoadFromXml(path);
        randomGenerator = new Random();
    }

    public List<double> Test(List<double> inputs)
    {
        if (inputs.Count != Layers[0].Neurons.Count)
        {
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + inputs.Count
                                                                       + " values into neural layer with " +
                                                                       Layers[0].Neurons.Count + " neurons. "
                                                                       + "Consider using another network, or another descriptors.");
        }
        else
        {
            return activities(inputs);
        }
    }

    public void Learn(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
    {
        if (trainingSet.Pairs.Count == 0)
        {
            throw new NullReferenceException(
                "[Error] NN-Learn: You are using an empty training set, neural network couldn't be trained.");
        }
        else if (trainingSet.Pairs[0].Inputs.Count != Layers[0].Neurons.Count)
        {
            throw new IndexOutOfRangeException(
                "[Error] NN-Test: You are trying to pass vector with " + trainingSet.Pairs[0].Inputs
                    .Count + " values into neural layer with " + Layers[0].Neurons.Count
                + " neurons. Consider using another network, or another " + "descriptors.");
        }
        else if (trainingSet.Pairs[0].Outputs.Count != getLayer(Layers.Count - 1)
                     .Neurons.Count)
        {
            throw new IndexOutOfRangeException(
                "[Error] NN-Test:  You are trying to pass vector with " + trainingSet.Pairs[0].Inputs
                    .Count + " values into neural layer with " + Layers[0].Neurons.Count
                + " neurons. Consider using another network, or another " + "descriptors.");
        }
        else
        {
            adaptation(trainingSet, maxK, eps, lambda, micro);
        }
    }

    public void SaveToXml(string fileName)
    {
        logger.LogInformation("Saving network topology to file " + fileName);

        XmlDocument doc = new XmlDocument();

        XmlElement root = doc.CreateElement("neuralNetwork");
        root.SetAttribute("dateOfExport", DateTime.Now.ToString());
        XmlElement layers = doc.CreateElement("structure");
        layers.SetAttribute("numberOfLayers", Layers.Count.ToString());

        for (int il = 0; il < Layers.Count; il++)
        {
            NeuralLayer layerObj = Layers[il];
            XmlElement layer = doc.CreateElement("layer");
            layer.SetAttribute("index", il.ToString());
            layer.SetAttribute("numberOfNeurons", layerObj.Neurons.Count.ToString());

            for (int inIdx = 0; inIdx < layerObj.Neurons.Count; inIdx++)
            {
                Neuron neuron = layerObj.Neurons[inIdx];
                XmlElement neuronElement = doc.CreateElement("neuron");
                neuronElement.SetAttribute("index", inIdx.ToString());
                neuronElement.SetAttribute("NumberOfInputs", neuron.Inputs.Count.ToString());
                neuronElement.SetAttribute("threshold", neuron.Threshold.ToString());

                for (int ii = 0; ii < neuron.Inputs.Count; ii++)
                {
                    NeuralInput input = neuron.Inputs[ii];
                    XmlElement inputElement = doc.CreateElement("input");
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

        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = System.Text.Encoding.GetEncoding("iso-8859-2")
            };
            using (XmlWriter writer = XmlWriter.Create(fs, settings))
            {
                doc.Save(writer);
            }
        }
    }

    public void printNeuralNetwork()
    {
        for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
        {
            Console.WriteLine("Layer " + layerIndex);
            for (int neuronIndex = 0; neuronIndex < getLayer(layerIndex).Neurons.Count; neuronIndex++)
            {
                Console.Write("      Neuron " + neuronIndex + " (threshold=" + getLayer(layerIndex)
                    .Neurons[neuronIndex].Threshold + ") : ");
                for (int inputIndex = 0;
                     inputIndex < getLayer(layerIndex).Neurons[neuronIndex].Inputs.Count;
                     inputIndex++)
                {
                    Console.Write(getLayer(layerIndex).Neurons[neuronIndex].Inputs[inputIndex].Weight + " ");
                }

                Console.WriteLine();
            }
        }
    }


    #region Private Helpers

    private void LoadFromXml(string path)
    {
        logger.LogDebug("Loading network topology from InputStream");

        XmlDocument doc = new XmlDocument();

        try
        {
            doc.Load(path);

            if (doc == null)
            {
                throw new NullReferenceException();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }

        XmlNode nodeNeuralNetwork = doc.DocumentElement;

        if (nodeNeuralNetwork.Name != "neuralNetwork")
        {
            logger.LogError("Parse error in XML file, neural network couldn't be loaded.");
            return;
        }

        XmlNodeList nodeNeuralNetworkContent = nodeNeuralNetwork.ChildNodes;

        foreach (XmlNode nodeStructure in nodeNeuralNetworkContent)
        {
            if (nodeStructure.Name == "structure")
            {
                XmlNodeList nodeStructureContent = nodeStructure.ChildNodes;

                foreach (XmlNode nodeLayer in nodeStructureContent)
                {
                    if (nodeLayer.Name == "layer")
                    {
                        NeuralLayer neuralLayer = new NeuralLayer(this);
                        Layers.Add(neuralLayer);

                        XmlNodeList nodeLayerContent = nodeLayer.ChildNodes;

                        foreach (XmlNode nodeNeuron in nodeLayerContent)
                        {
                            if (nodeNeuron.Name == "neuron")
                            {
                                Neuron neuron = new Neuron(
                                    double.Parse(((XmlElement)nodeNeuron).GetAttribute("threshold")),
                                    neuralLayer);
                                neuralLayer.Neurons.Add(neuron);

                                XmlNodeList nodeNeuronContent = nodeNeuron.ChildNodes;

                                foreach (XmlNode nodeNeuralInput in nodeNeuronContent)
                                {
                                    if (nodeNeuralInput.Name == "input")
                                    {
                                        logger.LogDebug("neuron at STR: {0} LAY: {1} NEU: {2} INP: {3}",
                                            Layers.Count - 1, neuralLayer.Neurons.Count - 1,
                                            neuralLayer.Neurons.IndexOf(neuron),
                                            neuron.Inputs.Count);

                                        NeuralInput neuralInput = new NeuralInput(
                                            double.Parse(((XmlElement)nodeNeuralInput).GetAttribute("weight")),
                                            neuron);
                                        neuron.Inputs.Add(neuralInput);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private double random() { return randomGenerator.NextDouble(); }

    private void computeGradient(Gradients gradients, List<Double> inputs, List<Double> requiredOutputs)
    {
        activities(inputs);
        for (int layerIndex = Layers.Count - 1;
             layerIndex >= 1;
             layerIndex--)
        {
            // backpropagation cez vsetky vrstvy okrem poslednej
            NeuralLayer currentLayer = getLayer(layerIndex);
            if (currentLayer.IsTopLayer)
            {
                // ak sa jedna o najvyssiu vrstvu
                // pridame gradient prahov pre danu vrstvu do odpovedajuceho
                // vektora a tento gradient pocitame cez neurony:
                // gradients.Thresholds.Add(layerIndex, new ArrayList<Double>());
                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    Neuron currentNeuron = currentLayer.Neurons[neuronIndex];
                    gradients.SetThreshold(layerIndex, neuronIndex,
                        currentNeuron.Output * (1 - currentNeuron.Output) *
                        (currentNeuron.Output - requiredOutputs[neuronIndex]));
                }

                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    Neuron currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (int inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                    {
                        NeuralInput currentInput = currentNeuron.Inputs[inputIndex];
                        gradients.SetWeight(layerIndex, neuronIndex, inputIndex,
                            gradients.GetThreshold(layerIndex, neuronIndex) * currentLayer.LowerLayer()
                                .Neurons[inputIndex].Output);
                    }
                }
            }
            else
            {
                // ak sa jedna o spodnejsie vrstvy (najnizsiu vrstvu
                // nepocitame, ideme len po 1.)
                // pocitame gradient prahov :
                // gradients.Thresholds.Add(layerIndex, new ArrayList<Double>());
                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    double aux = 0;
                    // iterujeme cez vsetky axony neuronu (resp. synapsie neuronov na vyssej vrstve)
                    for (int axonIndex = 0; axonIndex < currentLayer.UpperLayer().Neurons.Count; axonIndex++)
                    {
                        aux += gradients.GetThreshold(layerIndex + 1, axonIndex) * currentLayer.UpperLayer()
                            .Neurons[axonIndex].Inputs[neuronIndex].Weight;
                    }

                    gradients.SetThreshold(layerIndex, neuronIndex,
                        currentLayer.Neurons[neuronIndex].Output * (1 - currentLayer
                            .Neurons[neuronIndex].Output) * aux);
                }

                // pocitame gradienty vah :
                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    Neuron currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (int inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                    {
                        NeuralInput currentInput = currentNeuron.Inputs[inputIndex];
                        gradients.SetWeight(layerIndex, neuronIndex, inputIndex,
                            gradients.GetThreshold(layerIndex, neuronIndex) * currentLayer.LowerLayer()
                                .Neurons[inputIndex].Output);
                    }
                }
            }
        }
    }

    private void computeTotalGradient(Gradients totalGradients, Gradients partialGradients, SetOfIOPairs trainingSet)
    {
        totalGradients.ResetGradients();
        foreach (SetOfIOPairs.IOPair pair in trainingSet.Pairs)
        {
            computeGradient(partialGradients, pair.Inputs, pair.Outputs);
            for (int layerIndex = Layers.Count - 1;
                 layerIndex >= 1;
                 layerIndex--)
            {
                // all layers except last one
                NeuralLayer currentLayer = getLayer(layerIndex);
                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    // upravime gradient prahov :
                    totalGradients.IncrementThreshold(layerIndex, neuronIndex,
                        partialGradients.GetThreshold(layerIndex, neuronIndex));
                    for (int inputIndex = 0; inputIndex < currentLayer.LowerLayer().Neurons.Count; inputIndex++)
                    {
                        totalGradients.IncrementWeight(layerIndex, neuronIndex, inputIndex,
                            partialGradients.GetWeight(layerIndex, neuronIndex, inputIndex));
                    }
                }
            }
        }
    }

    private void adaptation(SetOfIOPairs trainingSet, int maxK, double eps, double lambda, double micro)
    {
        double delta;
        Gradients deltaGradients = new Gradients(this);
        Gradients totalGradients = new Gradients(this);
        Gradients partialGradients = new Gradients(this);
        logger.LogDebug("Setting up random weights and thresholds ...");
        // prahy a vahy neuronovej siete nastavime na nahodne hodnoty,
        // delta-gradienty vynulujeme (oni sa nuluju uz pri init)
        for (int layerIndex = Layers.Count - 1;
             layerIndex >= 1;
             layerIndex--)
        {
            // top down all layers except last one
            NeuralLayer currentLayer = getLayer(layerIndex);
            for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
            {
                Neuron currentNeuron = currentLayer.Neurons[neuronIndex];
                currentNeuron.Threshold = (2 * random()) - 1;
                for (int inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                {
                    currentNeuron.Inputs[inputIndex].Weight = (2 * random()) - 1;
                }
            }
        }

        int curK = 0;
        double curE = Double.PositiveInfinity; // pociatocna aktualna presnost bude nekonecna (tendencia znizovania)
        logger.LogDebug("Entering adaptation loop ... (maxK = " + maxK + ")");
        while ((curK < maxK) && (curE > eps))
        {
            computeTotalGradient(totalGradients, partialGradients, trainingSet);
            for (int layerIndex = Layers.Count - 1;
                 layerIndex >= 1;
                 layerIndex--)
            {
                // top down all layers except last one
                NeuralLayer currentLayer = getLayer(layerIndex);
                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    Neuron currentNeuron = currentLayer.Neurons[neuronIndex];
                    delta = (-lambda * totalGradients.GetThreshold(layerIndex, neuronIndex)) + (micro * deltaGradients
                        .GetThreshold(layerIndex, neuronIndex));
                    currentNeuron.Threshold += delta;
                    deltaGradients.SetThreshold(layerIndex, neuronIndex, delta);
                }

                for (int neuronIndex = 0; neuronIndex < currentLayer.Neurons.Count; neuronIndex++)
                {
                    Neuron currentNeuron = currentLayer.Neurons[neuronIndex];
                    for (int inputIndex = 0; inputIndex < currentNeuron.Inputs.Count; inputIndex++)
                    {
                        delta = (-lambda * totalGradients.GetWeight(layerIndex, neuronIndex, inputIndex)) + (micro
                            * deltaGradients.GetWeight(layerIndex, neuronIndex, inputIndex));
                        currentNeuron.Inputs[inputIndex].Weight += delta;
                        deltaGradients.SetWeight(layerIndex, neuronIndex, inputIndex, delta);
                    }
                }
            }

            curE = totalGradients.GetGradientAbs();
            curK++;
            if ((curK % 25) == 0)
            {
                logger.LogDebug("curK=" + curK + ", curE=" + curE);
            }
        }
    }

    private List<Double> activities(List<Double> inputs)
    {
        for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
        {
            for (int neuronIndex = 0; neuronIndex < getLayer(layerIndex).Neurons.Count; neuronIndex++)
            {
                double sum = getLayer(layerIndex).Neurons[neuronIndex].Threshold; // sum <- threshold
                for (int inputIndex = 0;
                     inputIndex < getLayer(layerIndex).Neurons[neuronIndex].Inputs.Count;
                     inputIndex++)
                {
                    // vstupy
                    // vynasobi vahu so vstupom
                    if (layerIndex == 0)
                    {
                        // ak sme na najspodnejsej vrstve, nasobime vahy so vstupmi
                        sum += getLayer(layerIndex).Neurons[neuronIndex].Inputs[inputIndex].Weight *
                               inputs[neuronIndex];
                    }
                    else
                    {
                        // na hornych vrstvach nasobime vahy s vystupmi nizsej vrstvy
                        sum += getLayer(layerIndex).Neurons[neuronIndex].Inputs[inputIndex]
                            .Weight * getLayer(layerIndex - 1).Neurons[inputIndex].Output;
                    }
                }

                getLayer(layerIndex).Neurons[neuronIndex].Output = gainFunction(sum);
            }
        }

        List<Double> Output = new();
        for (int i = 0; i < getLayer(Layers.Count - 1).Neurons.Count; i++)
        {
            Output.Add(getLayer(Layers.Count - 1).Neurons[i].Output);
        }

        return Output;
    }

    private double gainFunction(double x) { return 1 / (1 + Math.Exp(-x)); }

    private NeuralLayer getLayer(int index) { return Layers[index]; }

    #endregion
}
