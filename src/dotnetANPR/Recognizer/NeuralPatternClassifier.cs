using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using Microsoft.Extensions.Logging;
using NN = dotnetANPR.NeuralNetwork;

namespace dotnetANPR.Recognizer;

internal sealed class NeuralPatternClassifier : CharacterRecognizer
{
    private readonly AnprSettings _settings;
    private readonly ILogger _logger;

    public NN.NeuralNetwork NeuralNetwork { get; private set; }

    public NeuralPatternClassifier(AnprSettings settings, ILogger logger) : this(settings, logger, false) { }

    public NeuralPatternClassifier(AnprSettings settings, ILogger logger, bool learn)
    {
        _settings = settings;
        _logger = logger;

        List<int> dimensions = [];
        var inputLayerSize = settings.CharFeaturesExtractionMethod == 0
            ? settings.CharNormalizedDimensionsX * settings.CharNormalizedDimensionsY
            : Features.Length * 4;

        dimensions.Add(inputLayerSize);
        dimensions.Add(settings.NeuralTopology);
        dimensions.Add(Alphabet.Length);
        NeuralNetwork = new NN.NeuralNetwork(dimensions);

        if (learn)
        {
            try
            {
                LearnAlphabet(settings.CharLearnAlphabetPath, settings);
            }
            catch (IOException e)
            {
                _logger.LogError(e, "Failed to load alphabet: {Path}", settings.CharLearnAlphabetPath);
            }
        }
        else
        {
            NeuralNetwork = new NN.NeuralNetwork(settings.CharNeuralNetworkPath);
        }
    }

    public override RecognizedCharacter Recognize(Character character, AnprSettings settings)
    {
        character.Normalize(settings);
        var output = NeuralNetwork.Test(character.ExtractFeatures(settings));
        RecognizedCharacter recognized = new();

        for (var i = 0; i < output.Count; i++)
            recognized.AddPattern(new RecognizedPattern(Alphabet[i], (float)output[i]));

        recognized.Sort(true);
        return recognized;
    }

    public NN.SetOfIOPairs.IOPair CreateNewPair(char chr, Character imgChar, AnprSettings settings)
    {
        var vectorInput = imgChar.ExtractFeatures(settings);
        var vectorOutput = Alphabet.Select(alphabet => chr == alphabet ? 1.0 : 0.0).ToList();
        return new NN.SetOfIOPairs.IOPair(vectorInput, vectorOutput);
    }

    public void LearnAlphabet(string folder, AnprSettings settings)
    {
        var train = new NN.SetOfIOPairs();
        var fileList = Character.AlphabetList(folder);
        foreach (var fileName in fileList)
        {
            using var imgChar = new Character(fileName, settings);
            imgChar.Normalize(settings);
            train.AddIOPair(CreateNewPair(fileName.ToUpper()[0], imgChar, settings));
        }

        NeuralNetwork.Learn(
            train,
            settings.NeuralMaxK,
            settings.NeuralEps,
            settings.NeuralLambda,
            settings.NeuralMicro);
    }
}
