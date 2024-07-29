using System.Collections.Generic;

namespace DotNetANPR.NeuralNetwork;

public class NeuralNetwork
{
    public List<NeuralLayer> Layers { get; } = new();

    public NeuralNetwork(List<int> dimensions) { }

    public NeuralNetwork(string path) { }

    public static List<double> Test(List<double> extractFeatures) { throw new System.NotImplementedException(); }

    public void Learn(SetOfIOPairs train, int get, double d, double get1, double d1)
    {
        throw new System.NotImplementedException();
    }
    
    #region Private Helpers
    
    #endregion

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
}
