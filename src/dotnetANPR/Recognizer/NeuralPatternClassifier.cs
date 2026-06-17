using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using NN = DotNetANPR.NeuralNetwork;

namespace DotNetANPR.Recognizer;

/// <summary>
/// Neural network-based pattern classifier that recognizes characters by feeding their
/// extracted features through a multi-layer perceptron and mapping the network output
/// to alphabet characters.
/// </summary>
public class NeuralPatternClassifier : CharacterRecognizer
{
    private static readonly int NormalizeX = AnprConfig.Instance.Character.NormalizedWidth;
    private static readonly int NormalizeY = AnprConfig.Instance.Character.NormalizedHeight;

    /// <summary>
    /// Gets or sets the underlying neural network used for classification.
    /// </summary>
    public NN.NeuralNetwork NeuralNetwork { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="NeuralPatternClassifier"/> that loads a pre-trained
    /// network from the configured XML file.
    /// </summary>
    public NeuralPatternClassifier() : this(false) { }

    /// <summary>
    /// Initializes a new <see cref="NeuralPatternClassifier"/>.
    /// </summary>
    /// <param name="learn">
    /// When <c>true</c>, trains a new network from alphabet images;
    /// when <c>false</c>, loads a pre-trained network from XML.
    /// </param>
    public NeuralPatternClassifier(bool learn)
    {
        var config = AnprConfig.Instance;
        var dimensions = new List<int>();

        // Determine size of input layer according to chosen feature extraction method
        var inputLayerSize = config.Character.FeaturesExtractionMethod == 0
            ? NormalizeX * NormalizeY
            : Features.Length * 4;

        // Construct new neural network with specified dimensions
        dimensions.Add(inputLayerSize);
        dimensions.Add(config.NeuralNetwork.Topology);
        dimensions.Add(Alphabet.Length);
        NeuralNetwork = new NN.NeuralNetwork(dimensions);

        if (learn)
        {
            var learnAlphabetPath = config.Character.LearnAlphabetPath;
            try
            {
                LearnAlphabet(learnAlphabetPath);
            }
            catch (IOException)
            {
                throw;
            }
        }
        else
        {
            // Load pre-trained network from XML
            var neuralNetPath = config.Character.NeuralNetworkPath
                .Replace('/', System.IO.Path.DirectorySeparatorChar)
                .Replace('\\', System.IO.Path.DirectorySeparatorChar);
            NeuralNetwork = new NN.NeuralNetwork(neuralNetPath);
        }
    }

    /// <summary>
    /// Recognizes a character image by normalizing it, extracting features, and feeding
    /// them through the neural network. The output neurons are mapped to alphabet characters.
    /// </summary>
    /// <param name="character">The character image to recognize.</param>
    /// <returns>
    /// A <see cref="RecognizedCharacter"/> with patterns sorted by descending confidence.
    /// </returns>
    public override RecognizedCharacter Recognize(Character character)
    {
        character.Normalize();
        var output = NeuralNetwork.Test(character.ExtractFeatures());
        var recognized = new RecognizedCharacter();

        for (var i = 0; i < output.Count; i++)
            recognized.AddPattern(new RecognizedPattern(Alphabet[i], (float)output[i]));

        recognized.Render();
        recognized.Sort(true);
        return recognized;
    }

    /// <summary>
    /// Creates an input/output pair for training the neural network from a character image.
    /// The output vector is a one-hot encoding of the character's position in the alphabet.
    /// </summary>
    /// <param name="chr">The target character label.</param>
    /// <param name="imgChar">The normalized character image.</param>
    /// <returns>An <see cref="NN.SetOfIOPairs.IOPair"/> suitable for training.</returns>
    public NN.SetOfIOPairs.IOPair CreateNewPair(char chr, Character imgChar)
    {
        var vectorInput = imgChar.ExtractFeatures();
        var vectorOutput = Alphabet.Select(a => chr == a ? 1.0 : 0.0).ToList();
        return new NN.SetOfIOPairs.IOPair(vectorInput, vectorOutput);
    }

    /// <summary>
    /// Trains the neural network with alphabet images found in the specified directory.
    /// Each image file's first character determines which alphabet character it represents.
    /// </summary>
    /// <param name="folder">The path to the alphabet image directory.</param>
    public void LearnAlphabet(string folder)
    {
        var train = new NN.SetOfIOPairs();
        var fileList = Character.AlphabetList(folder);

        foreach (var fileName in fileList)
        {
            var imgChar = new Character(fileName);
            imgChar.Normalize();
            train.AddIOPair(CreateNewPair(Path.GetFileName(fileName).ToUpper()[0], imgChar));
        }

        NeuralNetwork.Learn(train,
            AnprConfig.Instance.NeuralNetwork.MaxK,
            AnprConfig.Instance.NeuralNetwork.Eps,
            AnprConfig.Instance.NeuralNetwork.Lambda,
            AnprConfig.Instance.NeuralNetwork.Micro);
    }
}
