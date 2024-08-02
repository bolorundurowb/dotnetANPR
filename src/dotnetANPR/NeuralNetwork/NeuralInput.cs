namespace DotNetANPR.NeuralNetwork;

public class NeuralInput(double weight, Neuron neuron)
{
    public double Weight { get; set; } = weight;

    public Neuron Neuron { get; } = neuron;

    public int Index { get; } = neuron.Inputs.Count;
}
