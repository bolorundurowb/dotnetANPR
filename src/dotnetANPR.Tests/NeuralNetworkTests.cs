using System.Collections.Generic;
using DotNetANPR.Utilities;
using OmniAssert;
using Xunit;
using NN = DotNetANPR.NeuralNetwork;

namespace DotNetANPR.Tests;

public class NeuralNetworkTests
{
    [Fact]
    public void Constructor_CreatesLayersFromDimensions()
    {
        var network = new NN.NeuralNetwork(new List<int> { 2, 3, 1 });

        network.Layers.Count.Verify().ToBe(3);
        network.Layers[0].Neurons.Count.Verify().ToBe(2);
        network.Layers[1].Neurons.Count.Verify().ToBe(3);
        network.Layers[2].Neurons.Verify().ToHaveCount(1);
    }

    [Fact]
    public void Test_ReturnsOutputForInput()
    {
        var network = new NN.NeuralNetwork(new List<int> { 2, 3, 1 });

        var output = network.Test(new List<double> { 0.5, 0.5 });

        output.Verify().ToHaveCount(1);
        output[0].Verify().ToBeInRange(0, 1);
    }

    [Fact]
    public void Constructor_LoadsFromEmbeddedResource()
    {
        using var stream = ResourceHelper.OpenStream("Resources/neuralnetworks/network_avgres_813_map.xml");

        stream.Verify().NotToBeNull();
        var network = new NN.NeuralNetwork(stream!);

        network.Layers.Verify().NotToBeEmpty();
    }
}
