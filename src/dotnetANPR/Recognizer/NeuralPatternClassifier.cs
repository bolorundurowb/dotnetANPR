using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using Microsoft.Extensions.Logging;
using NN = DotNetANPR.NeuralNetwork;

namespace DotNetANPR.Recognizer;

public class NeuralPatternClassifier : CharacterRecognizer
{
    private static readonly ILogger<NeuralPatternClassifier> Logger =
        LoggerFactory.Create(_ => { }).CreateLogger<NeuralPatternClassifier>();

    private static readonly int NormalizeX = Configurator.Instance.Get<int>("char_normalizeddimensions_x");
    private static readonly int NormalizeY = Configurator.Instance.Get<int>("char_normalizeddimensions_y");

    /// <summary>
    /// The dimensions of an input character after transformation are 10 * 16 = 160 neurons.
    /// </summary>
    public NN.NeuralNetwork NeuralNetwork { get; private set; }

    /// <summary>
    /// Do not learn the network, but load it from a file (default).
    /// </summary>
    public NeuralPatternClassifier() : this(false) { }

    public NeuralPatternClassifier(bool learn)
    {
        var configurator = Configurator.Instance;
        List<int> dimensions = [];
        // determine size of input layer according to chosen feature extraction method
        var inputLayerSize = configurator.Get<int>("char_featuresExtractionMethod") == 0
            ? NormalizeX * NormalizeY
            : Features.Length * 4;

        // construct new neural network with specified dimensions
        dimensions.Add(inputLayerSize);
        dimensions.Add(configurator.Get<int>("neural_topology"));
        dimensions.Add(Alphabet.Length);
        NeuralNetwork = new NN.NeuralNetwork(dimensions);
        if (learn)
        {
            var learnAlphabetPath = configurator.Get<string>("char_learnAlphabetPath");
            try
            {
                LearnAlphabet(learnAlphabetPath);
            }
            catch (IOException e)
            {
                Logger.LogError(e, "Failed to load alphabet: {}", learnAlphabetPath);
            }
        }
        else
        {
            // or load network from xml
            var neuralNetPath = configurator.GetPath("char_neuralNetworkPath");
            NeuralNetwork = new NN.NeuralNetwork(neuralNetPath);
        }
    }

    public override RecognizedCharacter Recognize(Character character)
    {
        character.Normalize();
        var output = NeuralNetwork.Test(character.ExtractFeatures());
        RecognizedCharacter recognized = new();

        for (var i = 0; i < output.Count; i++)
            recognized.AddPattern(new RecognizedPattern(Alphabet[i], (float)output[i]));

        recognized.Render();
        recognized.Sort(true);
        return recognized;
    }

    /// <summary>
    /// Creates a new IOPair with the given character and normalized image character.
    /// </summary>
    /// <param name="chr">The character.</param>
    /// <param name="imgChar">The normalized image character.</param>
    /// <returns>An IOPair object.</returns>
    public NN.SetOfIOPairs.IOPair CreateNewPair(char chr, Character imgChar)
    {
        var vectorInput = imgChar.ExtractFeatures();
        var vectorOutput = Alphabet.Select(alphabet => chr == alphabet ? 1.0 : 0.0).ToList();

        return new NN.SetOfIOPairs.IOPair(vectorInput, vectorOutput);
    }

    /// <summary>
    /// Learn the neural network with an alphabet in the given folder.
    /// </summary>
    /// <param name="folder">The alphabet folder.</param>
    /// <exception cref="IOException">If the alphabet failed to load.</exception>
    public void LearnAlphabet(string folder)
    {
        var train = new NN.SetOfIOPairs();
        var fileList = Character.AlphabetList(folder);
        foreach (var fileName in fileList)
        {
            var imgChar = new Character(fileName);
            imgChar.Normalize();
            train.AddIOPair(CreateNewPair(fileName.ToUpper()[0], imgChar));
        }

        NeuralNetwork.Learn(train, Configurator.Instance.Get<int>("neural_maxk"),
            Configurator.Instance.Get<double>("neural_eps"),
            Configurator.Instance.Get<double>("neural_lambda"),
            Configurator.Instance.Get<double>("neural_micro"));
    }
}
