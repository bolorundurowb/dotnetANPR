using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dotNETANPR.GUI;
using dotNETANPR.Recognizer;
using dotNETANPR.ImageAnalysis;


namespace dotNETANPR.Intelligence
{
    public class Intelligence
    {
        private long lastProcessDuration = 0; // trvanie posledneho procesu v ms

        public static Configurator.Configurator configurator = new Configurator.Configurator("." + Path.DirectorySeparatorChar + "config.xml");
        public static CharacterRecognizer chrRecog;
        public static Parser parser;
        public bool enableReportGeneration;

        public Intelligence(bool enableReportGeneration)
        {
            this.enableReportGeneration = enableReportGeneration;
            int classification_method = configurator.getIntProperty("intelligence_classification_method");

            if (classification_method == 0)
                chrRecog = new KnnPatternClassificator();
            else
                chrRecog = new NeuralPatternClassificator();

            parser = new Parser();
        }

        // vrati ako dlho v ms trvalo posledne rozpoznaavnie
        public long LastProcessDuration()
        {
            return lastProcessDuration;
        }

        public String recognize(CarSnapshot carSnapshot)
    {
        TimeMeter time = new TimeMeter();
        int syntaxAnalysisMode = Intelligence.configurator.getIntProperty("intelligence_syntaxanalysis");
        int skewDetectionMode = Intelligence.configurator.getIntProperty("intelligence_skewdetection");
        
        if (enableReportGeneration)
        {
            Main.rg.insertText("<h1>Automatic Number Plate Recognition Report</h1>");
            Main.rg.insertText("<span>Image width: "+carSnapshot.getWidth()+" px</span>");
            Main.rg.insertText("<span>Image height: "+carSnapshot.getHeight()+" px</span>");
            
            Main.rg.insertText("<h2>Vertical and Horizontal plate projection</h2>");
            
            Main.rg.insertImage(carSnapshot.renderGraph(), "snapshotgraph",0,0);
            Main.rg.insertImage(carSnapshot.getBiWithAxes(), "snapshot",0,0);
        }
        
        foreach (Band b in carSnapshot.getBands())
        { //doporucene 3

            if (enableReportGeneration) 
            {
                Main.rg.insertText("<div class='bandtxt'><h4>Band<br></h4>");
                Main.rg.insertImage(b.getBi(),"bandsmall", 250,30);
                Main.rg.insertText("<span>Band width : "+b.getWidth()+" px</span>");
                Main.rg.insertText("<span>Band height : "+b.getHeight()+" px</span>");
                Main.rg.insertText("</div>");
            }
            
            foreach (Plate plate in b.getPlates()) 
            {//doporucene 3

                if (enableReportGeneration) 
                {
                    Main.rg.insertText("<div class='platetxt'><h4>Plate<br></h4>");
                    Main.rg.insertImage(plate.getBi(),"platesmall", 120, 30);
                    Main.rg.insertText("<span>Plate width : "+plate.getWidth()+" px</span>");
                    Main.rg.insertText("<span>Plate height : "+plate.getHeight()+" px</span>");
                    Main.rg.insertText("</div>");
                }                

                
                // SKEW-RELATED
                Plate notNormalizedCopy = null;
                BufferedImage renderedHoughTransform = null;
                HoughTransformation hough = null;
                if (enableReportGeneration || skewDetectionMode!=0) 
                { // detekcia sa robi but 1) koli report generatoru 2) koli korekcii
                    notNormalizedCopy = plate.clone();
                    notNormalizedCopy.horizontalEdgeDetector(notNormalizedCopy.getBi());
                    hough = notNormalizedCopy.getHoughTransformation(); 
                    renderedHoughTransform = hough.render(HoughTransformation.RENDER_ALL, HoughTransformation.COLOR_BW);
                }
                if (skewDetectionMode!=0) 
                { // korekcia sa robi iba ak je zapnuta
                    AffineTransform shearTransform = AffineTransform.getShearInstance(0,-(double)hough.dy/hough.dx);
                    BufferedImage core = plate.createBlankBi(plate.getBi());
                    core.createGraphics().drawRenderedImage(plate.getBi(), shearTransform);
                    plate = new Plate(core);
                }
                
                plate.normalize();
                
                float plateWHratio = (float)plate.getWidth() / (float)plate.getHeight();
                if (plateWHratio < Intelligence.configurator.getDoubleProperty("intelligence_minPlateWidthHeightRatio")
                ||  plateWHratio > Intelligence.configurator.getDoubleProperty("intelligence_maxPlateWidthHeightRatio")
                ) continue;
                
                Vector<Char> chars = plate.getChars();
                
                
                // heuristicka analyza znacky z pohladu uniformity a poctu pismen :
                //Recognizer.configurator.getIntProperty("intelligence_minimumChars")
                if (chars.size() < Intelligence.configurator.getIntProperty("intelligence_minimumChars") ||
                        chars.size() > Intelligence.configurator.getIntProperty("intelligence_maximumChars")
                        ) continue;
                
                if (plate.getCharsWidthDispersion(chars) > Intelligence.configurator.getDoubleProperty("intelligence_maxCharWidthDispersion")
                ) continue;
                
                /* ZNACKA PRIJATA, ZACINA NORMALIZACIA A HEURISTIKA PISMEN */

                if (enableReportGeneration)
                {
                    Main.rg.insertText("<h2>Detected band</h2>");
                    Main.rg.insertImage(b.getBiWithAxes(),"band",0,0);
                    Main.rg.insertImage(b.renderGraph(),"bandgraph",0,0);
                    Main.rg.insertText("<h2>Detected plate</h2>");
                    Plate plateCopy = plate.clone();
                    plateCopy.linearResize(450, 90);
                    Main.rg.insertImage(plateCopy.getBiWithAxes(), "plate",0,0);
                    Main.rg.insertImage(plateCopy.renderGraph(), "plategraph",0,0);
                }
                
                // SKEW-RELATED
                if (enableReportGeneration)
                {
                    Main.rg.insertText("<h2>Skew detection</h2>");
                    //Main.rg.insertImage(notNormalizedCopy.getBi());                    
                    Main.rg.insertImage(notNormalizedCopy.getBi(), "skewimage",0,0);
                    Main.rg.insertImage(renderedHoughTransform, "skewtransform",0,0);
                    Main.rg.insertText("Detected skew angle : <b>"+hough.angle+"</b>" );
                }
                                    
                
                RecognizedPlate recognizedPlate = new RecognizedPlate();
                
                if (enableReportGeneration) 
                {
                    Main.rg.insertText("<h2>Character segmentation</h2>");
                    Main.rg.insertText("<div class='charsegment'>");
                    for (ImageAnalysis.Char chr in chars) 
                    {
                        Main.rg.insertImage(Photo.linearResizeBi(chr.getBi(),70,100), "",0,0);
                    }
                    Main.rg.insertText("</div>");
                }
                
                for (ImageAnalysis.Char chr in chars) chr.normalize();
                
                float averageHeight = plate.getAveragePieceHeight(chars);
                float averageContrast = plate.getAveragePieceContrast(chars);
                float averageBrightness = plate.getAveragePieceBrightness(chars);
                float averageHue = plate.getAveragePieceHue(chars);
                float averageSaturation = plate.getAveragePieceSaturation(chars);
                
                for (ImageAnalysis.Char chr in chars) 
                {
                    // heuristicka analyza jednotlivych pismen
                    bool ok = true;
                    String errorFlags = "";
                    
                    // pri normalizovanom pisme musime uvazovat pomer
                    float widthHeightRatio = (float)(chr.pieceWidth);
                    widthHeightRatio /= (float)(chr.pieceHeight);
                    
                    if (widthHeightRatio < Intelligence.configurator.getDoubleProperty("intelligence_minCharWidthHeightRatio") ||
                            widthHeightRatio > Intelligence.configurator.getDoubleProperty("intelligence_maxCharWidthHeightRatio")
                            )
                    {
                        errorFlags += "WHR ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    
                    
                    if ((chr.positionInPlate.x1 < 2 ||
                            chr.positionInPlate.x2 > plate.getWidth()-1)
                            && widthHeightRatio < 0.12
                            ) 
                    {
                        errorFlags += "POS ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    
                    
                    //float similarityCost = rc.getSimilarityCost();
                    
                    float contrastCost = Math.abs(chr.statisticContrast - averageContrast);
                    float brightnessCost = Math.abs(chr.statisticAverageBrightness - averageBrightness);
                    float hueCost = Math.abs(chr.statisticAverageHue - averageHue);
                    float saturationCost = Math.abs(chr.statisticAverageSaturation - averageSaturation);
                    float heightCost = (chr.pieceHeight - averageHeight) / averageHeight;
                    
                    if (brightnessCost > Intelligence.configurator.getDoubleProperty("intelligence_maxBrightnessCostDispersion")) 
                    {
                        errorFlags += "BRI ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    if (contrastCost > Intelligence.configurator.getDoubleProperty("intelligence_maxContrastCostDispersion")) 
                    {
                        errorFlags += "CON ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    if (hueCost > Intelligence.configurator.getDoubleProperty("intelligence_maxHueCostDispersion"))
                    {
                        errorFlags += "HUE ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    if (saturationCost > Intelligence.configurator.getDoubleProperty("intelligence_maxSaturationCostDispersion")) 
                    {
                        errorFlags += "SAT ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    if (heightCost < -Intelligence.configurator.getDoubleProperty("intelligence_maxHeightCostDispersion")) 
                    {
                        errorFlags += "HEI ";
                        ok = false;
                        if (!enableReportGeneration) continue;
                    }
                    
                    float similarityCost = 0;
                    RecognizedChar rc = null;
                    if (ok==true)
                    {
                        rc = this.chrRecog.recognize(chr);
                        similarityCost = rc.getPatterns().elementAt(0).getCost();
                        if (similarityCost > Intelligence.configurator.getDoubleProperty("intelligence_maxSimilarityCostDispersion")) 
                        {
                            errorFlags += "NEU ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }
                        
                    }
                    
                    if (ok==true)
                    {
                        recognizedPlate.addChar(rc);
                    } 
                    else 
                    {
                    }
                    
                    if (enableReportGeneration) 
                    {
                        Main.rg.insertText("<div class='heuristictable'>");
                        Main.rg.insertImage(Photo.linearResizeBi(chr.getBi(),chr.getWidth()*2, chr.getHeight()*2), "skeleton",0,0);
                        Main.rg.insertText("<span class='name'>WHR</span><span class='value'>"+widthHeightRatio+"</span>");
                        Main.rg.insertText("<span class='name'>HEI</span><span class='value'>"+heightCost+"</span>");
                        Main.rg.insertText("<span class='name'>NEU</span><span class='value'>"+similarityCost+"</span>");
                        Main.rg.insertText("<span class='name'>CON</span><span class='value'>"+contrastCost+"</span>");
                        Main.rg.insertText("<span class='name'>BRI</span><span class='value'>"+brightnessCost+"</span>");
                        Main.rg.insertText("<span class='name'>HUE</span><span class='value'>"+hueCost+"</span>");
                        Main.rg.insertText("<span class='name'>SAT</span><span class='value'>"+saturationCost+"</span>");
                        Main.rg.insertText("</table>");
                        if (errorFlags.Length()!=0) Main.rg.insertText("<span class='errflags'>"+errorFlags+"</span>");
                        Main.rg.insertText("</div>");
                    }
                } // end for each char
                
                
                // nasledujuci riadok zabezpeci spracovanie dalsieho kandidata na znacku, v pripade ze charrecognizingu je prilis malo rozpoznanych pismen
                if (recognizedPlate.chars.size() < Intelligence.configurator.getIntProperty("intelligence_minimumChars")) continue;
                
                this.lastProcessDuration = time.getTime();
                String parsedOutput = parser.parse(recognizedPlate, syntaxAnalysisMode);
                
                if (enableReportGeneration)
                {
                    Main.rg.insertText("<span class='recognized'>");
                    Main.rg.insertText("Recognized plate : "+parsedOutput);
                    Main.rg.insertText("</span>");
                }
                
                return parsedOutput;
            
            } // end for each  plate

        }
        
        this.lastProcessDuration = time.getTime();
        //return new String("not available yet ;-)");
        return null;
    }
    }
}
