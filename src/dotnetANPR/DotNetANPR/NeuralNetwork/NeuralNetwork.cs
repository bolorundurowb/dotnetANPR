using System.Collections.Generic;

namespace DotNetANPR.NeuralNetwork;

public class NeuralNetwork
{
    public NeuralNetwork(List<int> dimensions) { }

    public NeuralNetwork(string path) { }

    public static List<double> Test(List<double> extractFeatures) { throw new System.NotImplementedException(); }

    public void Learn(SetOfIOPairs train, int get, double d, double get1, double d1)
    {
        throw new System.NotImplementedException();
    }

    public class SetOfIOPairs
    {
        public void AddIOPair(IOPair createNewPair) { throw new System.NotImplementedException(); }

        public class IOPair
        {
            public IOPair(List<double> vectorInput, List<double> vectorOutput)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
