using System.Collections.Generic;

namespace DotNetANPR.NeuralNetwork;

public class SetOfIOPairs
{
    public List<IOPair> Pairs { get; } = new();

    public void AddIOPair(List<double> inputs, List<double> outputs) => AddIOPair(new IOPair(inputs, outputs));

    public void AddIOPair(IOPair pair) => Pairs.Add(pair);

    public class IOPair(List<double> inputs, List<double> outputs)
    {
        public List<double> Inputs { get; } = inputs;

        public List<double> Outputs { get; } = outputs;
    }
}
