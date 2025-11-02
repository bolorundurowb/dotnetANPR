using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNetANPR.NeuralNetwork
{
    public class NeuralNetworkModel
    {
        [JsonPropertyName("layers")]
        public List<NeuralLayerModel> Layers { get; set; }
    }

    public class NeuralLayerModel
    {
        [JsonPropertyName("neurons")]
        public List<NeuronModel> Neurons { get; set; }
    }

    public class NeuronModel
    {
        [JsonPropertyName("weights")]
        public double[] Weights { get; set; }
        
        // Internal state for computation
        [JsonIgnore]
        public double Output { get; set; }
    }
}