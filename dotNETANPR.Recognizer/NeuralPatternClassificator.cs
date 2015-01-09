using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotNETANPR.NeuralNetwork;
using System.IO;

namespace dotNETANPR.Recognizer
{
    public class NeuralPatternClassificator : CharacterRecognizer
    {
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static int normalize_x =
            configg.getIntProperty("char_normalizeddimensions_x");
    private static int normalize_y =
            configg.getIntProperty("char_normalizeddimensions_y");
    
    // rozmer vstupneho pismena po transformacii : 10 x 16 = 160 neuronov
    
    public NeuralNetwork.NeuralNetwork network;
    
    // do not learn netwotk, but load if from file (default)
    public NeuralPatternClassificator() 
    {
        new NeuralPatternClassificator(false);
    }
    
    public NeuralPatternClassificator(bool learn)
    {
        // zakomentovane dna 2.1.2007
        // this.normalize_x = Intelligence.configurator.getIntProperty("char_normalizeddimensions_x");
        // this.normalize_y = Intelligence.configurator.getIntProperty("char_normalizeddimensions_y");
        
        List<Int32> dimensions = new List<Int32>();
        
        // determine size of input layer according to chosen feature extraction method.
        int inputLayerSize;
        if (configg.getIntProperty("char_featuresExtractionMethod")==0)
            inputLayerSize = normalize_x * normalize_y;
        else inputLayerSize = CharacterRecognizer.features.Length*4;
        
        // construct new neural network with specified dimensions.
        dimensions.Add(inputLayerSize);
        dimensions.Add(configg.getIntProperty("neural_topology"));
        dimensions.Add(CharacterRecognizer.alphabet.Length);
        this.network = new NeuralNetwork.NeuralNetwork(dimensions);
        
        if (learn)
        {
            // learn network
            learnAlphabet(configg.getStrProperty("char_learnAlphabetPath"));
        }
        else 
        {
            // or load network from xml
            this.network = new NeuralNetwork.NeuralNetwork(configg.getPathProperty("char_neuralNetworkPath"));
        }
    }
    
    // IMAGE -> CHAR
    public RecognizedChar recognize(ImageAnalysis.Char imgChar) 
    { // rozpozna UZ normalizovany char
        imgChar.normalize();
        List<Double> output = this.network.test(imgChar.extractFeatures());
        double max = 0.0;
        int indexMax = 0;
        
        RecognizedChar recognized = new RecognizedChar();
        
        for (int i=0; i<output.Count; i++)
        {
            recognized.addPattern(new RecognizedChar.RecognizedPattern(alphabet[i], (float)output.ElementAt(i)));
        }
        recognized.render();
        recognized.sort(1);
        return recognized;
    }
    
//    public List<Double> imageToList(Char imgChar) {
//        List<Double> vectorInput = new List<Double>();
//        for (int x = 0; x<imgChar.getWidth(); x++)
//            for (int y = 0; y<imgChar.getHeight(); y++)
//                vectorInput.Add(new Double(imgChar.getBrightness(x,y)));
//        return vectorInput;
//    }
    public NeuralNetwork.NeuralNetwork.SetOfIOPairs.IOPair createNewPair(char chr, Char imgChar) { // uz normalizonvany
        List<Double> vectorInput = imgChar.extractFeatures();
        
        
        
        List<Double> vectorOutput = new List<Double>();
        for (int i=0; i<alphabet.Length; i++)
            if (chr == alphabet[i]) vectorOutput.Add(1.0); else vectorOutput.Add(0.0);
        
/*        System.out.println();
        for (Double d : vectorInput) System.out.print(d+" ");
        System.out.println();
        for (Double d : vectorOutput) System.out.print(d+" ");
        System.out.println();
 */
        
        return (new NeuralNetwork.NeuralNetwork.SetOfIOPairs.IOPair(vectorInput, vectorOutput));
    }
    
    // NAUCI NEURONOVU SIET ABECEDE, KTORU NAJDE V ADRESARI PATH
    public void learnAlphabet(String path)
    {
        String alphaString = "0123456789abcdefghijklmnopqrstuvwxyz";
        string[] files = Directory.GetFiles(path);
        NeuralNetwork.NeuralNetwork.SetOfIOPairs train = new NeuralNetwork.NeuralNetwork.SetOfIOPairs();
        
        foreach (String fileName in files)
        {
            if (alphaString.IndexOf(fileName.ToLower()[0])==-1)
                continue; // je to nezname meno suboru, skip

            ImageAnalysis.Char imgChar = new ImageAnalysis.Char(path + Path.DirectorySeparatorChar + fileName);
            imgChar.normalize();
            train.AddIOPair(this.createNewPair(fileName.ToUpper()[0], imgChar));
        }
        
        this.network.learn(train,
                configg.getIntProperty("neural_maxk"),
                configg.getDoubleProperty("neural_eps"),
                configg.getDoubleProperty("neural_lambda"),
                configg.getDoubleProperty("neural_micro")
                );
    }
    }
}
