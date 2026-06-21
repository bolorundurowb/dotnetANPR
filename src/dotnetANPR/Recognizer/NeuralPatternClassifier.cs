using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Utilities;
using Microsoft.Extensions.Logging;
using NN = dotnetANPR.NeuralNetwork;

namespace dotnetANPR.Recognizer;

/// <summary>
/// Character recognition using a feed-forward neural network.
/// The network can be loaded from a pre-trained file or trained on an alphabet at runtime.
/// </summary>
public class NeuralPatternClassifier : CharacterRecognizer
{
    private static readonly ILogger<NeuralPatternClassifier> Logger = Logging.GetLogger<NeuralPatternClassifier>();

    private static readonly int NormalizeX = Configurator.Instance.Get<int>("char_normalizeddimensions_x");
    private static readonly int NormalizeY = Configurator.Instance.Get<int>("char_normalizeddimensions_y");

    /// <summary>
    /// Gets the underlying neural network used for classification.
    /// </summary>
    public NN.NeuralNetwork NeuralNetwork { get; private set; }

    /// <summary>
    /// Initialises the classifier by loading a pre-trained network from the configured XML path.
    /// </summary>
    public NeuralPatternClassifier() : this(false) { }

    /// <summary>
    /// Initialises the classifier. When <paramref name="learn"/> is <c>true</c>, the network is trained
    /// on the configured alphabet instead of loading from file.
    /// </summary>
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

    /// <summary>
    /// Recognises a character by normalising it and running it through the neural network.
    /// </summary>
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
    /// Creates an input-output training pair for the given character and its normalised image.
    /// </summary>
    /// <param name="chr">The target character (0-9, A-Z).</param>
    /// <param name="imgChar">The normalised image of the character.</param>
    /// <returns>An input-output pair where the output vector targets the correct character class.</returns>
    public NN.SetOfIOPairs.IOPair CreateNewPair(char chr, Character imgChar)
    {
        var vectorInput = imgChar.ExtractFeatures();
        var vectorOutput = Alphabet.Select(alphabet => chr == alphabet ? 1.0 : 0.0).ToList();

        return new NN.SetOfIOPairs.IOPair(vectorInput, vectorOutput);
    }

    /// <summary>
    /// Trains the neural network using the alphabet images in the specified folder.
    /// </summary>
    /// <param name="folder">The path to the alphabet training images directory.</param>
    /// <exception cref="IOException">Thrown if the alphabet failed to load.</exception>
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
