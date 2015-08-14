using System;
using System.Linq;
using dotNETANPR.Recognizer;
using System.IO;
using dotNETANPR.ImageAnalysis;
using dotNETANPR.GUI;
using System.Drawing;

namespace dotNETANPR.Intelligence
{
    class Intelligence
    {
        private long lastProcessDuration = 0;

        public static Configurator.Configurator configurator = new Configurator.Configurator("." + Path.DirectorySeparatorChar + "config.xml");
        public static CharacterRecognizer chrRecog;
        public static Parser parser;
        public bool enableReportGeneration;

        public Intelligence(bool enableReportGeneration)
        {
            this.enableReportGeneration = enableReportGeneration;
            int classification_method = configurator.GetIntProperty("intelligence_classification_method");

            if (classification_method == 0)
                chrRecog = new KnnPatternClassificator();
            else
                chrRecog = new NeuralPatternClassificator();

            parser = new Parser();
        }

        public long LastProcessDuration()
        {
            return lastProcessDuration;
        }

        public String recognize(CarSnapshot carSnapshot)
        {
            TimeMeter time = new TimeMeter();
            int syntaxAnalysisMode = Intelligence.configurator.GetIntProperty("intelligence_syntaxanalysis");
            int skewDetectionMode = Intelligence.configurator.GetIntProperty("intelligence_skewdetection");

            if (enableReportGeneration)
            {
                CMain.rg.InsertText("<h1>Automatic Number Plate Recognition Report</h1>");
                CMain.rg.InsertText("<span>Image width: " + carSnapshot.GetWidth() + " px</span>");
                CMain.rg.InsertText("<span>Image height: " + carSnapshot.GetHeight() + " px</span>");

                CMain.rg.InsertText("<h2>Vertical and Horizontal plate projection</h2>");

                CMain.rg.InsertImage(carSnapshot.RenderGraph(), "snapshotgraph", 0, 0);
                CMain.rg.InsertImage(carSnapshot.GetBiWithAxes(), "snapshot", 0, 0);
            }

            foreach (Band b in carSnapshot.GetBands())
            {
                if (enableReportGeneration)
                {
                    CMain.rg.InsertText("<div class='bandtxt'><h4>Band<br></h4>");
                    CMain.rg.InsertImage(b.GetBi(), "bandsmall", 250, 30);
                    CMain.rg.InsertText("<span>Band width : " + b.GetWidth() + " px</span>");
                    CMain.rg.InsertText("<span>Band height : " + b.GetHeight() + " px</span>");
                    CMain.rg.InsertText("</div>");
                }

                foreach (Plate plate in b.GetPlates())
                {
                    if (enableReportGeneration)
                    {
                        CMain.rg.InsertText("<div class='platetxt'><h4>Plate<br></h4>");
                        CMain.rg.InsertImage(plate.GetBi(), "platesmall", 120, 30);
                        CMain.rg.InsertText("<span>Plate width : " + plate.GetWidth() + " px</span>");
                        CMain.rg.InsertText("<span>Plate height : " + plate.GetHeight() + " px</span>");
                        CMain.rg.InsertText("</div>");
                    }


                    Plate notNormalizedCopy = null;
                    Bitmap renderedHoughTransform = null;
                    HoughTransformation hough = null;
                    if (enableReportGeneration || skewDetectionMode != 0)
                    {
                        notNormalizedCopy = plate.clone();
                        notNormalizedCopy.horizontalEdgeDetector(notNormalizedCopy.GetBi());
                        hough = notNormalizedCopy.GetHoughTransformation();
                        renderedHoughTransform = hough.render(HoughTransformation.RENDER_ALL, HoughTransformation.COLOR_BW);
                    }
                    if (skewDetectionMode != 0)
                    {
                        AffineTransform shearTransform = AffineTransform.GetShearInstance(0, -(double)hough.dy / hough.dx);
                        Bitmap core = plate.createBlankBi(plate.GetBi());
                        core.createGraphics().drawRenderedImage(plate.GetBi(), shearTransform);
                        plate = new Plate(core);
                    }

                    plate.Normalize();

                    float plateWHratio = (float)plate.GetWidth() / (float)plate.GetHeight();
                    if (plateWHratio < Intelligence.configurator.GetDoubleProperty("intelligence_minPlateWidthHeightRatio")
                    || plateWHratio > Intelligence.configurator.GetDoubleProperty("intelligence_maxPlateWidthHeightRatio")
                    )
                        continue;

                    Vector<ImageAnalysis.Char> chars = plate.GetChars();

                    if (chars.size() < Intelligence.configurator.GetIntProperty("intelligence_minimumChars") ||
                            chars.size() > Intelligence.configurator.GetIntProperty("intelligence_maximumChars")
                            )
                        continue;

                    if (plate.GetCharsWidthDispersion(chars) > Intelligence.configurator.GetDoubleProperty("intelligence_maxCharWidthDispersion")
                    )
                        continue;

                    if (enableReportGeneration)
                    {
                        CMain.rg.InsertText("<h2>Detected band</h2>");
                        CMain.rg.InsertImage(b.GetBiWithAxes(), "band", 0, 0);
                        CMain.rg.InsertImage(b.renderGraph(), "bandgraph", 0, 0);
                        CMain.rg.InsertText("<h2>Detected plate</h2>");
                        Plate plateCopy = plate.clone();
                        plateCopy.linearResize(450, 90);
                        CMain.rg.InsertImage(plateCopy.GetBiWithAxes(), "plate", 0, 0);
                        CMain.rg.InsertImage(plateCopy.renderGraph(), "plategraph", 0, 0);
                    }

                    if (enableReportGeneration)
                    {
                        CMain.rg.InsertText("<h2>Skew detection</h2>");
                        CMain.rg.InsertImage(notNormalizedCopy.GetBi(), "skewimage", 0, 0);
                        CMain.rg.InsertImage(renderedHoughTransform, "skewtransform", 0, 0);
                        CMain.rg.InsertText("Detected skew angle : <b>" + hough.angle + "</b>");
                    }


                    RecognizedPlate recognizedPlate = new RecognizedPlate();

                    if (enableReportGeneration)
                    {
                        CMain.rg.InsertText("<h2>Character segmentation</h2>");
                        CMain.rg.InsertText("<div class='charsegment'>");
                        foreach (ImageAnalysis.Char chr in chars)
                        {
                            CMain.rg.InsertImage(Photo.linearResizeBi(chr.GetBi(), 70, 100), "", 0, 0);
                        }
                        CMain.rg.InsertText("</div>");
                    }

                    foreach (ImageAnalysis.Char chr in chars)
                        chr.Normalize();

                    float averageHeight = plate.GetAveragePieceHeight(chars);
                    float averageContrast = plate.GetAveragePieceContrast(chars);
                    float averageBrightness = plate.GetAveragePieceBrightness(chars);
                    float averageHue = plate.GetAveragePieceHue(chars);
                    float averageSaturation = plate.GetAveragePieceSaturation(chars);

                    foreach (ImageAnalysis.Char chr in chars)
                    {
                        bool ok = true;
                        string errorFlags = "";

                        float widthHeightRatio = (float)(chr.pieceWidth);
                        widthHeightRatio /= (float)(chr.pieceHeight);

                        if (widthHeightRatio < Intelligence.configurator.GetDoubleProperty("intelligence_minCharWidthHeightRatio") ||
                                widthHeightRatio > Intelligence.configurator.GetDoubleProperty("intelligence_maxCharWidthHeightRatio")
                                )
                        {
                            errorFlags += "WHR ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }


                        if ((chr.positionInPlate.x1 < 2 ||
                                chr.positionInPlate.x2 > plate.GetWidth() - 1)
                                && widthHeightRatio < 0.12
                                )
                        {
                            errorFlags += "POS ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }


                        //float similarityCost = rc.GetSimilarityCost();

                        float contrastCost = Math.Abs(chr.statisticContrast - averageContrast);
                        float brightnessCost = Math.Abs(chr.statisticAverageBrightness - averageBrightness);
                        float hueCost = Math.Abs(chr.statisticAverageHue - averageHue);
                        float saturationCost = Math.Abs(chr.statisticAverageSaturation - averageSaturation);
                        float heightCost = (chr.pieceHeight - averageHeight) / averageHeight;

                        if (brightnessCost > Intelligence.configurator.GetDoubleProperty("intelligence_maxBrightnessCostDispersion"))
                        {
                            errorFlags += "BRI ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }
                        if (contrastCost > Intelligence.configurator.GetDoubleProperty("intelligence_maxContrastCostDispersion"))
                        {
                            errorFlags += "CON ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }
                        if (hueCost > Intelligence.configurator.GetDoubleProperty("intelligence_maxHueCostDispersion"))
                        {
                            errorFlags += "HUE ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }
                        if (saturationCost > Intelligence.configurator.GetDoubleProperty("intelligence_maxSaturationCostDispersion"))
                        {
                            errorFlags += "SAT ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }
                        if (heightCost < -Intelligence.configurator.GetDoubleProperty("intelligence_maxHeightCostDispersion"))
                        {
                            errorFlags += "HEI ";
                            ok = false;
                            if (!enableReportGeneration) continue;
                        }

                        float similarityCost = 0;
                        CharacterRecognizer.RecognizedChar rc = null;
                        if (ok == true)
                        {
                            rc = chrRecog.Recognize(chr);
                            similarityCost = rc.GetPatterns().ElementAt(0).GetCost();
                            if (similarityCost > configurator.GetDoubleProperty("intelligence_maxSimilarityCostDispersion"))
                            {
                                errorFlags += "NEU ";
                                ok = false;
                                if (!enableReportGeneration) continue;
                            }

                        }

                        if (ok == true)
                        {
                            recognizedPlate.AddChar(rc);
                        }
                        else
                        {
                        }

                        if (enableReportGeneration)
                        {
                            CMain.rg.InsertText("<div class='heuristictable'>");
                            CMain.rg.InsertImage(Photo.linearResizeBi(chr.GetBi(), chr.GetWidth() * 2, chr.GetHeight() * 2), "skeleton", 0, 0);
                            CMain.rg.InsertText("<span class='name'>WHR</span><span class='value'>" + widthHeightRatio + "</span>");
                            CMain.rg.InsertText("<span class='name'>HEI</span><span class='value'>" + heightCost + "</span>");
                            CMain.rg.InsertText("<span class='name'>NEU</span><span class='value'>" + similarityCost + "</span>");
                            CMain.rg.InsertText("<span class='name'>CON</span><span class='value'>" + contrastCost + "</span>");
                            CMain.rg.InsertText("<span class='name'>BRI</span><span class='value'>" + brightnessCost + "</span>");
                            CMain.rg.InsertText("<span class='name'>HUE</span><span class='value'>" + hueCost + "</span>");
                            CMain.rg.InsertText("<span class='name'>SAT</span><span class='value'>" + saturationCost + "</span>");
                            CMain.rg.InsertText("</table>");
                            if (errorFlags.Length() != 0) CMain.rg.InsertText("<span class='errflags'>" + errorFlags + "</span>");
                            CMain.rg.InsertText("</div>");
                        }
                    }

                    if (recognizedPlate.chars.size() < Intelligence.configurator.GetIntProperty("intelligence_minimumChars")) continue;

                    this.lastProcessDuration = time.GetTime();
                    String parsedOutput = parser.parse(recognizedPlate, syntaxAnalysisMode);

                    if (enableReportGeneration)
                    {
                        CMain.rg.InsertText("<span class='recognized'>");
                        CMain.rg.InsertText("Recognized plate : " + parsedOutput);
                        CMain.rg.InsertText("</span>");
                    }

                    return parsedOutput;

                }
            }

            lastProcessDuration = time.GetTime();
            return null;
        }
    }
}

