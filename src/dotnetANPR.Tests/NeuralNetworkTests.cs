using System.Collections.Generic;
using DotNetANPR.Utilities;
using Xunit;
using NN = DotNetANPR.NeuralNetwork;

namespace DotNetANPR.Tests;

public class NeuralNetworkTests
{
    [Fact]
    public void Constructor_CreatesLayersFromDimensions()
    {
        var network = new NN.NeuralNetwork(new List<int> { 2, 3, 1 });

        Assert.Equal(3, network.Layers.Count);
        Assert.Equal(2, network.Layers[0].Neurons.Count);
        Assert.Equal(3, network.Layers[1].Neurons.Count);
        Assert.Single(network.Layers[2].Neurons);
    }

    [Fact]
    public void Test_ReturnsOutputForInput()
    {
        var network = new NN.NeuralNetwork(new List<int> { 2, 3, 1 });

        var output = network.Test(new List<double> { 0.5, 0.5 });

        Assert.Single(output);
        Assert.InRange(output[0], 0, 1);
    }

    [Fact]
    public void Constructor_LoadsFromEmbeddedResource()
    {
        using var stream = ResourceHelper.OpenStream("Resources/neuralnetworks/network_avgres_813_map.xml");

        Assert.NotNull(stream);
        var network = new NN.NeuralNetwork(stream);

        Assert.NotEmpty(network.Layers);
    }
}
