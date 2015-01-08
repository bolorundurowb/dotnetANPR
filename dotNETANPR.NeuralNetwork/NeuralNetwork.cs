using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Threading.Tasks;
using System.Xml; 
using System.IO; 

namespace dotNETANPR.NeuralNetwork
{
    public class NeuralNetwork
    {
        // holds list of layers   
    private List < NeuralLayer> listLayers = new List < NeuralLayer>(); 
    private Random randomGenerator; 
    
    // Dimensions in order from the lowermost (input) to the uppermost (output) layer
    public NeuralNetwork(List < Int32> dimensions) 
    {
        // initialization of layers
        for (int i=0; i < dimensions.Count; i++)
        {
            this.listLayers.Add(new NeuralLayer(Convert.ToInt32(dimensions[i]),  this)); 
        }
        randomGenerator = new Random(); 
    }
    
    public NeuralNetwork(String fileName)
    {
        loadFromXml(fileName); 
        randomGenerator = new Random(); 
    }

    public List < Double> test (List < Double> inputs) 
    {
        if (inputs.Count != this.getLayer(0).numberOfNeurons()) throw new IndexOutOfRangeException("[Error] NN-Test: You are trying to pass vector with " + inputs.Count+" values into neural layer with " + this.getLayer(0).numberOfNeurons()+" neurons. Consider using another network,  or another descriptors."); 
        else return activities(inputs); 
    }
    
    public void learn (SetOfIOPairs trainingSet,  int maxK,  double eps,  double lambda,  double micro) 
    {
        if (trainingSet.pairs.Count==0) throw new NullReferenceException("[Error] NN-Learn: You are using an empty training set,  neural network couldn't be trained."); 
        else if (trainingSet.pairs[0].inputs.Count != this.getLayer(0).numberOfNeurons())
            throw new IndexOutOfRangeException("[Error] NN-Test: You are trying to pass vector with " + trainingSet.pairs[0].inputs.Count+" values into neural layer with " + this.getLayer(0).numberOfNeurons()+" neurons. Consider using another network,  or another descriptors."); 
        else if (trainingSet.pairs[0].outputs.Count != this.getLayer(this.numberOfLayers()-1).numberOfNeurons())
            throw new IndexOutOfRangeException("[Error] NN-Test:  You are trying to pass vector with " + trainingSet.pairs[0].inputs.Count+" values into neural layer with " + this.getLayer(0).numberOfNeurons()+" neurons. Consider using another network,  or another descriptors."); 
        else adaptation(trainingSet, maxK, eps, lambda, micro); 
    } 

    public int numberOfLayers() 
    {
        return this.listLayers.Count; 
    }
  
    private void loadFromXml(String fileName)
    {
        Console.WriteLine("NeuralNetwork : loading network topology from file " + fileName); 
        //DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance(); 
        //DocumentBuilder parser = factory.newDocumentBuilder(); 
        //Document doc = parser.parse(fileName); 
        
        XmlDocument doc = new XmlDocument(); 
        doc.Load(fileName); 

        XmlNode nodeNeuralNetwork = doc.DocumentElement; 
        if (!nodeNeuralNetwork.Name.Equals("neuralNetwork")) throw new ApplicationException("[Error] NN-Load: Parse error in XML file,  neural network couldn't be loaded."); 
        // nodeNeuralNetwork ok
        // indexNeuralNetworkContent -> indexStructureContent -> indexLayerContent -> indexNeuronContent -> indexNeuralInputContent
        XmlNodeList nodeNeuralNetworkContent = nodeNeuralNetwork.ChildNodes; 
        for (int innc=0; innc < nodeNeuralNetworkContent.Count; innc++) 
        {
            XmlNode nodeStructure = nodeNeuralNetworkContent.Item(innc); 
            if (nodeStructure.Name.Equals("structure")) 
            { // for structure element
                XmlNodeList nodeStructureContent = nodeStructure.ChildNodes; 
                for (int isc=0; isc < nodeStructureContent.Count; isc++) 
                {
                    XmlNode nodeLayer = nodeStructureContent.Item(isc); 
                    if (nodeLayer.Name.Equals("layer"))
                    { // for layer element
                        NeuralLayer neuralLayer = new NeuralLayer(this); 
                        this.listLayers.Add(neuralLayer); 
                        XmlNodeList nodeLayerContent = nodeLayer.ChildNodes; 
                        for (int ilc=0; ilc < nodeLayerContent.Count; ilc++)
                        {
                            XmlNode nodeNeuron = nodeLayerContent.Item(ilc); 
                            if (nodeNeuron.Name.Equals("neuron"))
                            { // for neuron in layer
                                Neuron neuron = new Neuron(Double.Parse(((XmlElement)nodeNeuron).GetAttribute("threshold")), neuralLayer); 
                                neuralLayer.listNeurons.Add(neuron); 
                                XmlNodeList nodeNeuronContent = nodeNeuron.ChildNodes; 
                                for (int inc=0; inc  <  nodeNeuronContent.Count; inc++)
                                {
                                    XmlNode nodeNeuralInput = nodeNeuronContent.Item(inc); 
                                    //if (nodeNeuralInput==null) System.out.print("-"); else System.out.print("*"); 
                                    
                                    if (nodeNeuralInput.Name.Equals("input"))
                                    {
//                                        Console.WriteLine("neuron at STR:" + innc+" LAY:" + isc+" NEU:" + ilc+" INP:" + inc); 
                                        NeuralInput neuralInput = new NeuralInput(Double.Parse(((XmlElement)nodeNeuralInput).GetAttribute("weight")), neuron); 
                                        neuron.listInputs.Add(neuralInput); 
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        

    }

    public void saveToXml(String fileName)
    {
        Console.WriteLine("Saving network topology to file " +  fileName); 
        //DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance(); 
        //DocumentBuilder parser = factory.newDocumentBuilder(); 
        XmlDocument doc = new XmlDocument(); 
        XmlElement root = doc.CreateElement("neuralNetwork"); 
        root.SetAttribute("dateOfExport",  new DateTime().Date.ToShortDateString()); 
        XmlElement layers = doc.CreateElement("structure"); 
        layers.SetAttribute("numberOfLayers", this.numberOfLayers().ToString()); 
        
        for (int il=0; il < this.numberOfLayers(); il++) 
        {
            XmlElement layer = doc.CreateElement("layer"); 
            layer.SetAttribute("index",  il.ToString()); 
            layer.SetAttribute("numberOfNeurons",  this.getLayer(il).numberOfNeurons().ToString()); 
            
            for (int ini=0; ini < this.getLayer(il).numberOfNeurons(); ini++) 
            {
                XmlElement neuron = doc.CreateElement("neuron"); 
                neuron.SetAttribute("index", ini.ToString()); 
                neuron.SetAttribute("NumberOfInputs", this.getLayer(il).getNeuron(ini).numberOfInputs().ToString()); 
                neuron.SetAttribute("threshold", this.getLayer(il).getNeuron(ini).threshold.ToString()); 
                
                for (int ii=0; ii < this.getLayer(il).getNeuron(ini).numberOfInputs(); ii++) 
                {
                    XmlElement input = doc.CreateElement("input"); 
                    input.SetAttribute("index", ii.ToString()); 
                    input.SetAttribute("weight", this.getLayer(il).getNeuron(ini).getInput(ii).weight.ToString()); 
                    
                    neuron.AppendChild(input); 
                }
                
                layer.AppendChild(neuron); 
            }
            
            layers.AppendChild(layer); 
        }
        
        root.AppendChild(layers); 
        doc.AppendChild(root); 
        
        // save
        FileStream fos = File.Create(fileName); 
        XmlWriter transformer; 
        
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.Encoding = Encoding.GetEncoding(28592);
        // transform source into result will do save
        //transformer.setOutputProperty("encoding", "iso-8859-2"); 
        transformer = XmlWriter.Create(fos, settings);
        doc.Save(transformer);
    }    
    
    


    public class SetOfIOPairs
    {
        public List <IOPair> pairs; 
        public  class IOPair 
        { // TU SOM PRIDAL STATIC,  posovdne do tam nebolo
            public List < Double> inputs; 
             public List < Double> outputs; 
            public IOPair(List < Double> inputs,  List < Double> outputs)
            {
                // corrected warning
                //this.inputs = (List < Double>)inputs.clone(); 
                //this.outputs = (List < Double>)outputs.clone(); 
                this.inputs = new List < Double>(inputs); 
                this.outputs = new List < Double>(outputs); 
            }
        }
        public SetOfIOPairs() 
        {
            this.pairs = new List <IOPair>(); 
        }
        public void AddIOPair(List <Double> inputs,  List <Double> outputs) 
        {
            this.AddIOPair(new IOPair(inputs, outputs)); 
        }
        public void AddIOPair(IOPair pair)
        {
            this.pairs.Add(pair); 
        }
        int Count() 
        {
            return pairs.Count; 
        }
    }

    private  class NeuralInput
    {
        public double weight; 
        int index; 
        Neuron neuron; 
        
        public NeuralInput(double weight,  Neuron neuron) 
        {
            this.neuron = neuron; 
            this.weight = weight; 
            this.index = this.neuron.numberOfInputs(); 
            //Console.WriteLine("Created neural input " + this.index+" with weight " + this.weight); 
        }
    } // end public class NeuralInput
    
        private  class Neuron 
        {
        public List < NeuralInput> listInputs = new List < NeuralInput>(); //holds list of inputs
        int index; 
        public double threshold; 
        public double output; 
        NeuralLayer neuralLayer; 
        
        // initializes all neuron weights to 1 parameter specifies number of weights
        
        public Neuron(double threshold,  NeuralLayer neuralLayer) 
        {
            this.threshold = threshold; 
            this.neuralLayer = neuralLayer; 
            this.index = this.neuralLayer.numberOfNeurons(); 
        }
        
        public Neuron(int numberOfInputs,  double threshold,  NeuralLayer neuralLayer)
        {
            this.threshold = threshold; 
            this.neuralLayer = neuralLayer; 
            this.index = this.neuralLayer.numberOfNeurons(); 
            for (int i=0; i < numberOfInputs; i++)
            {
                this.listInputs.Add(new NeuralInput(1.0, this)); 
            }
        }

        public int numberOfInputs() 
        {
            return this.listInputs.Count; 
        }
        
        public NeuralInput getInput (int index) 
        {
            return this.listInputs[index]; 
        }
        
    } // end public class Neuron

    private  class NeuralLayer 
    {
        //holds list od neurons
        public List < Neuron> listNeurons = new List < Neuron>(); 
        int index; 
        NeuralNetwork neuralNetwork; 
        
        public NeuralLayer(NeuralNetwork neuralNetwork)
        {
            this.neuralNetwork = neuralNetwork; 
            this.index = this.neuralNetwork.numberOfLayers(); 
        }
        
        // initializes all neurons in layer
        public NeuralLayer(int numberOfNeurons,  NeuralNetwork neuralNetwork)
        {
            this.neuralNetwork = neuralNetwork; 
            this.index = this.neuralNetwork.numberOfLayers(); 
            // ak sa jedna o najnizsiu vrstvu (0),  kazdy neuron bude mat iba 1 vstup
            for (int i=0; i < numberOfNeurons; i++)
            {
                if (this.index == 0) 
                {
                    this.listNeurons.Add(new Neuron(1, 0.0, this)); 
                    /* prahy neuronov najnizsej vrstvy su vzdy 0.0,  vrstva iba distribuuje vstupy,  aspon tak to vyplyva
                     * z algoritmu na strane 111 */
                } 
                else
                { // v opacnom pripade bude mat neuron tolko vstupov kolko je neuronov nizsej vrstvy
                    this.listNeurons.Add(
                                        /* prahy neuronou na vyssich vrstvach budu tiez 0.0,  ale nemusia byt */
                                        new Neuron(this.neuralNetwork.getLayer(this.index-1).numberOfNeurons(),  0.0,  this)
                                        ); 
                }
            }
            //Console.WriteLine("Created neural layer " + this.index+" with " + numberOfNeurons+" neurons"); 
        } // end constructor
        
        public int numberOfNeurons() 
        {
            return this.listNeurons.Count; 
        }
        
        public bool isLayerTop()
        {
            return (this.index == this.neuralNetwork.numberOfLayers()-1); 
        }
        
        public bool isLayerBottom()
        {
            return (this.index == 0); 
        }
        
        public NeuralLayer upperLayer() 
        { 
            if (this.isLayerTop()) return null; 
            return this.neuralNetwork.getLayer(index+1); 
        }

        public NeuralLayer lowerLayer()
        { 
            if (this.isLayerBottom()) return null; 
            return this.neuralNetwork.getLayer(index-1); 
        }
        
        public Neuron getNeuron(int index)
        {
            return this.listNeurons[index]; 
        }
        
    } // end public class NeuralLayer
    
    private  class Gradients
    {
        List < List < Double>> thresholds; 
        List < List < List < Double>>> weights; 
        NeuralNetwork neuralNetwork; 
        
        public Gradients(NeuralNetwork network) 
        {
            this.neuralNetwork = network; 
            this.initGradients(); 
        }
        
        public void initGradients()
        {
            this.thresholds = new List < List < Double>>(); 
            this.weights = new List < List < List < Double>>>(); 
            //Console.WriteLine("init for threshold gradient " + this.ToString()); 
            for (int il = 0; il  <  this.neuralNetwork.numberOfLayers(); il++) 
            {
                this.thresholds.Add(new List < Double>()); 
                this.weights.Add(new List < List < Double>>()); 
                for (int ini = 0; ini  <  this.neuralNetwork.getLayer(il).numberOfNeurons(); ini++)
                {
                    this.thresholds[il].Add(0.0); 
                    this.weights[il].Add(new List < Double>()); 
                    for (int ii = 0; ii  <  this.neuralNetwork.getLayer(il).getNeuron(ini).numberOfInputs(); ii++)
                    {
                        this.weights[il][ini].Add(0.0); 
                    } // for each input
                } // for each neuron
            } // for each layer
        }
        
        public void resetGradients() 
        { //resets to 0
           for (int il = 0; il  <  this.neuralNetwork.numberOfLayers(); il++) 
           {
               for (int ini = 0; ini  <  this.neuralNetwork.getLayer(il).numberOfNeurons(); ini++)
               { 
                   this.setThreshold(il, ini, 0.0); 
                   for (int ii = 0; ii  <  this.neuralNetwork.getLayer(il).getNeuron(ini).numberOfInputs(); ii++)
                   {
                       this.setWeight(il, ini, ii, 0.0); 
                   }
               }
           }
        }
        
        public double getThreshold(int il,  int ini) 
        {
            return Double.Parse(thresholds[il][ini].ToString()); 
        }

        public void setThreshold(int il,  int ini,  double value) 
        {
            thresholds[il][ini] = value; 
        }
        
        public void incrementThreshold(int il,  int ini,  double value)
        {
            this.setThreshold(il, ini, this.getThreshold(il, ini) + value); 
        }
       
        public double getWeight (int il,  int ini,  int ii) 
    {
            return weights[il][ini][ii];
        }
        
        public void setWeight (int il,  int ini,  int ii,  double value)
    {
            weights[il][ini][ii] = value; 
        }
         
        public void incrementWeight(int il,  int ini,  int ii,  double value)
    {
            this.setWeight(il, ini, ii, this.getWeight(il, ini, ii) + value); 
        }

        public double getGradientAbs() 
        {
            double currE = 0; 
            
            for (int il=1; il < neuralNetwork.numberOfLayers(); il++) {
                currE += this.vectorAbs(thresholds[il]); 
                currE += this.doubleVectorAbs(weights[il]); 
            }
            return currE;            
            
            //for (List < Double> vector : this.thresholds) currE += this.vectorAbs(vector); 
            //for (List < List < Double>> doubleVector : this.weights) currE += this.doubleVectorAbs(doubleVector);    
            //return currE; 
        }
        
        private double doubleVectorAbs(List < List < Double>> doubleVector) 
        {
            double totalX = 0; 
            foreach (List < Double> vector in doubleVector) 
            {
                totalX += Math.Pow(vectorAbs(vector), 2); 
            }
            return Math.Sqrt(totalX); 
        }
        
        private double vectorAbs(List < Double> vector) 
        {
            double totalX = 0;
            foreach (Double x in vector)
            {
                totalX += Math.Pow(x, 2);
            }
            return Math.Sqrt(totalX); 
        }
        
    }
    
    private double random() 
    {
        return randomGenerator.NextDouble(); 
    }
    
    private void computeGradient(Gradients gradients,  List < Double> inputs,  List < Double> requiredOutputs) 
    {
       //Gradients gradients = new Gradients(this); 
       activities(inputs); 
       for (int il=this.numberOfLayers()-1; il>=1; il--)
       { 
           //backpropagation cez vsetky vrstvy okrem poslednej
            NeuralLayer currentLayer = this.getLayer(il); 
           
            if (currentLayer.isLayerTop())
            { 
                // ak sa jedna o najvyssiu vrstvu
                // pridame gradient prahov pre danu vrstvu do odpovedajuceho vektora a tento gradient pocitame cez neurony : 
                //gradients.thresholds.Add(il,  new List < Double>()); 
                for (int ini =0; ini < currentLayer.numberOfNeurons(); ini++) 
                { 
                    // pre vsetky neurony na vrstve
                    Neuron currentNeuron = currentLayer.getNeuron(ini); 
                    gradients.setThreshold(il,  ini, currentNeuron.output * (1 - currentNeuron.output) * (currentNeuron.output - requiredOutputs[ini])); 
                } // end for each neuron

                for (int ini=0; ini < currentLayer.numberOfNeurons(); ini++) 
                { 
                    // for each neuron
                    Neuron currentNeuron = currentLayer.getNeuron(ini); 
                    for (int ii=0; ii < currentNeuron.numberOfInputs(); ii++) 
                    { // for each neuron's input
                        NeuralInput currentInput = currentNeuron.getInput(ii); 
                        gradients.setWeight(il, ini, ii, 
                                gradients.getThreshold(il, ini) * currentLayer.lowerLayer().getNeuron(ii).output    
                                ); 
                    } // end for each input
                } // end for each neuron
                
            } 
            else 
            { 
                // ak sa jedna o spodnejsie vrstvy (najnizsiu vrstvu nepocitame,  ideme len po 1.) 
                // pocitame gradient prahov :
                //gradients.thresholds.Add(il,  new List < Double>()); 
                for (int ini =0; ini < currentLayer.numberOfNeurons(); ini++) 
                { // for each neuron
                    double aux = 0; 
                    // iterujeme cez vsetky axony neuronu (resp. synapsie neuronov na vyssej vrstve)
                    for (int ia=0; ia < currentLayer.upperLayer().numberOfNeurons(); ia++) { 
                        aux += gradients.getThreshold(il+1, ia) * 
                               currentLayer.upperLayer().getNeuron(ia).getInput(ini).weight; 
                    }
                    gradients.setThreshold(il, ini, 
                            currentLayer.getNeuron(ini).output * (1 - currentLayer.getNeuron(ini).output) * aux
                            ); 
                } //end for each neuron
                
                // pocitame gradienty vah : 
                for (int ini = 0; ini < currentLayer.numberOfNeurons(); ini++) 
                { // for each neuron
                    Neuron currentNeuron = currentLayer.getNeuron(ini); 
                    for (int ii=0; ii < currentNeuron.numberOfInputs(); ii++) 
                    { // for each neuron's input
                        NeuralInput currentInput = currentNeuron.getInput(ii); 
                        gradients.setWeight(il,  ini,  ii, 
                                gradients.getThreshold(il, ini) * currentLayer.lowerLayer().getNeuron(ii).output
                        );    
                    } // end for each input
                } // end for each neuron
               
            } // end layer IF
            
        } // end backgropagation for each layer
        //return gradients; 
    }
   
    private void computeTotalGradient(Gradients totalGradients,  Gradients partialGradients,  SetOfIOPairs trainingSet) 
    {
        // na zaciatku sa inicializuju gradienty (total)
        totalGradients.resetGradients(); 
        //partialGradients.resetGradients(); 
        //Gradients totalGradients = new Gradients(this); 
        //Gradients partialGradients = new Gradients(this); /***/
        
        foreach (SetOfIOPairs.IOPair pair in trainingSet.pairs) 
        {  
            computeGradient (partialGradients,  pair.inputs,  pair.outputs); 
            for (int il = this.numberOfLayers()-1; il >= 1; il--) 
            { 
                NeuralLayer currentLayer = this.getLayer(il); 
                for (int ini = 0; ini < currentLayer.numberOfNeurons(); ini++) 
                { 
                    
                    totalGradients.incrementThreshold(il, ini, partialGradients.getThreshold(il, ini)); 
                    for (int ii=0; ii < currentLayer.lowerLayer().numberOfNeurons(); ii++) 
                    { // pre vsetky vstupy
                        totalGradients.incrementWeight(il, ini, ii, partialGradients.getWeight(il, ini, ii)); 
                    }
                }
            
            } // end for layer
        } // end foreach
        //return totalGradients; 
    } // end method

    private void adaptation(SetOfIOPairs trainingSet,  int maxK,  double eps ,  double lambda ,  double micro) 
    {
//        
        double delta; 
        Gradients deltaGradients = new Gradients(this); 
        Gradients totalGradients = new Gradients(this); 
        Gradients partialGradients = new Gradients(this); 
        
        Console.WriteLine("setting up random weights and thresholds ..."); 
        
        // prahy a vahy neuronovej siete nastavime na nahodne hodnoty,  delta-gradienty vynulujeme (oni sa nuluju uz pri init)
        for (int il = this.numberOfLayers()-1; il >= 1; il--) 
        { // iteracia cez vsetky vrstvy nadol okrem poslednej
            NeuralLayer currentLayer = this.getLayer(il); 
            for (int ini = 0; ini < currentLayer.numberOfNeurons(); ini++)
            { // pre kazdy neuron na vrstve
                Neuron currentNeuron = currentLayer.getNeuron(ini); 
                currentNeuron.threshold = 2*this.random()-1; 
                //deltaGradients.setThreshold(il, ini, 0.0); 
                for (int ii = 0; ii  <  currentNeuron.numberOfInputs(); ii++) 
                {
                    currentNeuron.getInput(ii).weight = 2 * this.random()-1; 
                    //deltaGradients.setWeight(il, ini, ii, 0.0); 
                } // end ii
            } // end in
        } // end il
         
        int currK = 0; // citac iteracii
        double currE = Double.PositiveInfinity; // pociatocna aktualna presnost bude nekonecna (tendencia znizovania)
        
        Console.WriteLine("entering adaptation loop ... (maxK = " + maxK+")"); 
        
        while ( currK  <  maxK && currE > eps ) 
        {
            computeTotalGradient(totalGradients, partialGradients, trainingSet); 
            for (int il = this.numberOfLayers()-1; il >= 1; il--)
            { // iteracia cez vsetky vrstvy nadol okrem poslednej
                NeuralLayer currentLayer = this.getLayer(il);            
                
                for (int ini = 0; ini < currentLayer.numberOfNeurons(); ini++) 
                { // pre kazdy neuron na vrstve
                    Neuron currentNeuron = currentLayer.getNeuron(ini); 
                    delta = -lambda * totalGradients.getThreshold(il, ini) + micro * deltaGradients.getThreshold(il, ini); 
                    currentNeuron.threshold += delta; 
                    deltaGradients.setThreshold(il, ini, delta); 
                } // end for ii 1
                
                for (int ini = 0; ini < currentLayer.numberOfNeurons(); ini++) 
                { // pre kazdy neuron na vrstve
                    Neuron currentNeuron = currentLayer.getNeuron(ini); 
                    for (int ii = 0; ii  <  currentNeuron.numberOfInputs(); ii++) 
                    { // a pre kazdy vstup neuronu
                        delta = -lambda * totalGradients.getWeight(il, ini, ii) + micro * deltaGradients.getWeight(il, ini, ii); 
                        currentNeuron.getInput(ii).weight += delta; 
                        deltaGradients.setWeight(il, ini, ii, delta); 
                    } // end for ii
                } // end for in 2
            } // end for il
            
            currE = totalGradients.getGradientAbs(); 
            currK++; 
            if (currK%25==0) Console.WriteLine("currK=" + currK+"   currE=" + currE); 
        } // end while
    }
        
    private List < Double> activities (List < Double> inputs) 
    {
        for (int il=0; il < this.numberOfLayers(); il++) 
        { // pre kazdu vrstvu
            for (int ini = 0; ini < this.getLayer(il).numberOfNeurons(); ini++) 
            { // pre kazdy neuron vo vrstve
                double sum = this.getLayer(il).getNeuron(ini).threshold; // sum  < - threshold
                for (int ii=0; ii < this.getLayer(il).getNeuron(ini).numberOfInputs(); ii++) 
                { // vstupy
                    // vynasobi vahu so vstupom
                    if (il==0)
                    { // ak sme na najspodnejsej vrstve,  nasobime vahy so vstupmi
                        sum += this.getLayer(il).getNeuron(ini).getInput(ii).weight *
                        inputs[ini]; 
                    } 
                    else
                    { // na hornych vrstvach nasobime vahy s vystupmi nizsej vrstvy
                        sum+=
                        this.getLayer(il).getNeuron(ini).getInput(ii).weight *
                        this.getLayer(il-1).getNeuron(ii).output; 
                    }
                }
                
            
                      this.getLayer(il).getNeuron(ini).output = this.gainFunction(sum); 
                
                //this.getLayer(il).getNeuron(ini).output = this.gainFunction(sum); // vystup neuronu
            }
        }
        // nazaver vystupy neuronov najvyssej vrstvy zapiseme do vektora : 
        List < Double> output = new List < Double>(); 
        
        for (int i=0; i < this.getLayer(this.numberOfLayers()-1).numberOfNeurons(); i++) 
            output.Add(this.getLayer(this.numberOfLayers()-1).getNeuron(i).output); 
        
        return output; 
    }
        
    private double gainFunction (double x) 
    {
        return 1/(1+Math.Exp(-x)); 
    }
    
    private NeuralLayer getLayer(int index)
    {
        return this.listLayers[index]; 
    }
    

    }
}
