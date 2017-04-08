using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using dotNETANPR.Gui;
using dotNETANPR.ImageAnalysis;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class Intelligence
    {
        private long _lastProcessDuration;

        public static Configurator.Configurator Configurator { get; set; } = new Configurator.Configurator(
            "." +
            Path.DirectorySeparatorChar +
            "config.xml"
        );

        public static CharacterRecognizer ChrRecog;
        public static Parser Parser;
        public bool EnableReportGeneration;

        public Intelligence(bool enableReportGeneration)
        {
            EnableReportGeneration = enableReportGeneration;
            int classificationMethod = Configurator.GetIntProperty("intelligence_classification_method");
            if (classificationMethod == 0)
            {
                ChrRecog = new KnnPatternClassificator();
            }
            else
            {
                ChrRecog = new NeuralPatternClassificator();
            }
            Parser = new Parser();
        }

        public long LastProcessDuration()
        {
            return _lastProcessDuration;
        }

        public string Recognize(CarSnapshot carSnapshot)
        {
            TimeMeter timeMeter = new TimeMeter();
            int syntaxAnalysisMode = Configurator.GetIntProperty("intelligence_syntaxanalysis");
            int skewDetectionMode = Configurator.GetIntProperty("intelligence_skewdetection");

            if (EnableReportGeneration)
            {
                Program.ReportGenerator.InsertText("<h1>Automatic Number Plate Recognition Report</h1>");
                Program.ReportGenerator.InsertText("<span>Image width: " + carSnapshot.GetWidth() + " px</span>");
                Program.ReportGenerator.InsertText("<span>Image height: " + carSnapshot.GetHeight() + " px</span>");

                Program.ReportGenerator.InsertText("<h2>Vertical and Horizontal plate projection</h2>");

                Program.ReportGenerator.InsertImage(carSnapshot.RenderGraph(), "snapshotgraph", 0, 0);
                Program.ReportGenerator.InsertImage(carSnapshot.GetBitmapWithAxes(), "snapshot", 0, 0);
            }

            foreach (Band band in carSnapshot.GetBands())
            {
                if (EnableReportGeneration)
                {
                    Program.ReportGenerator.InsertText("<div class='bandtxt'><h4>Band<br></h4>");
                    Program.ReportGenerator.InsertImage(band.GetBitmap(), "bandsmall", 250, 30);
                    Program.ReportGenerator.InsertText("<span>Band width : " + band.GetWidth() + " px</span>");
                    Program.ReportGenerator.InsertText("<span>Band height : " + band.GetHeight() + " px</span>");
                    Program.ReportGenerator.InsertText("</div>");
                }

                foreach (Plate plate in band.GetPlates())
                {
                    var _plate = plate;
                    if (EnableReportGeneration)
                    {
                        Program.ReportGenerator.InsertText("<div class='_platetxt'><h4>Plate<br></h4>");
                        Program.ReportGenerator.InsertImage(_plate.GetBitmap(), "_platesmall", 120, 30);
                        Program.ReportGenerator.InsertText("<span>Plate width : " + _plate.GetWidth() + " px</span>");
                        Program.ReportGenerator.InsertText("<span>Plate height : " + _plate.GetHeight() + " px</span>");
                        Program.ReportGenerator.InsertText("</div>");
                    }


                    Plate notNormalizedCopy = null;
                    Bitmap renderedHoughTransform = null;
                    HoughTransformation hough = null;
                    if (EnableReportGeneration || skewDetectionMode != 0)
                    {
                        notNormalizedCopy = _plate.Clone();
                        notNormalizedCopy.HorizontalEdgeDetector(notNormalizedCopy.GetBitmap());
                        hough = notNormalizedCopy.GetHoughTransformation();
                        renderedHoughTransform =
                            hough.Render(HoughTransformation.RenderAll, HoughTransformation.ColorBw);
                    }
                    if (skewDetectionMode != 0)
                    {
                        Matrix matrix = new Matrix();
                        matrix.Shear(0f, - hough.Dy/ hough.Dx);
                        Bitmap core = _plate.CreateBlankBitmap(_plate.GetBitmap());
                        Graphics graphics = Graphics.FromImage(core);
                        graphics.Transform = matrix;
                        graphics.DrawImage(_plate.GetBitmap(), core.Height, core.Width);
                        _plate = new Plate(core);
                    }

                    _plate.Normalize();

                    float plateWHratio = _plate.GetWidth() / (float) _plate.GetHeight();
                    if (plateWHratio < Configurator.GetDoubleProperty("intelligence_minPlateWidthHeightRatio")
                        || plateWHratio > Configurator.GetDoubleProperty("intelligence_maxPlateWidthHeightRatio")
                    ) continue;

                    List<Character> chars = _plate.GetChars();

                    if (chars.Count < Configurator.GetIntProperty("intelligence_minimumChars") ||
                        chars.Count > Configurator.GetIntProperty("intelligence_maximumChars")
                    ) continue;

                    if (_plate.GetCharsWidthDispersion(chars) >
                        Configurator.GetDoubleProperty("intelligence_maxCharWidthDispersion")
                    ) continue;


                    if (EnableReportGeneration)
                    {
                        Program.ReportGenerator.InsertText("<h2>Detected band</h2>");
                        Program.ReportGenerator.InsertImage(band.GetBitmapWithAxes(), "band", 0, 0);
                        Program.ReportGenerator.InsertImage(band.RenderGraph(), "bandgraph", 0, 0);
                        Program.ReportGenerator.InsertText("<h2>Detected _plate</h2>");
                        Plate plateCopy = _plate.Clone();
                        plateCopy.LinearResize(450, 90);
                        Program.ReportGenerator.InsertImage(plateCopy.GetBitmapWithAxes(), "_plate", 0, 0);
                        Program.ReportGenerator.InsertImage(plateCopy.RenderGraph(), "_plategraph", 0, 0);
                    }

                    if (EnableReportGeneration)
                    {
                        Program.ReportGenerator.InsertText("<h2>Skew detection</h2>");
                        Program.ReportGenerator.InsertImage(notNormalizedCopy.GetBitmap(), "skewimage", 0, 0);
                        Program.ReportGenerator.InsertImage(renderedHoughTransform, "skewtransform", 0, 0);
                        Program.ReportGenerator.InsertText("Detected skew angle : <b>" + hough.Angle + "</b>");
                    }


                    RecognizedPlate recognizedPlate = new RecognizedPlate();

                    if (EnableReportGeneration)
                    {
                        Program.ReportGenerator.InsertText("<h2>Character segmentation</h2>");
                        Program.ReportGenerator.InsertText("<div class='charsegment'>");
                        foreach (Character chr in chars)
                        {
                            Program.ReportGenerator.InsertImage(Photo.LinearResizeBitmap(chr.GetBitmap(), 70, 100), "",
                                0, 0);
                        }
                        Program.ReportGenerator.InsertText("</div>");
                    }

                    foreach (Character chr in chars) chr.Normalize();

                    float averageHeight = _plate.GetAveragePieceHeight(chars);
                    float averageContrast = _plate.GetAveragePieceContrast(chars);
                    float averageBrightness = _plate.GetAveragePieceBrightness(chars);
                    float averageHue = _plate.GetAveragePieceHue(chars);
                    float averageSaturation = _plate.GetAveragePieceSaturation(chars);

                    foreach (Character chr in chars)
                    {
                        bool ok = true;
                        string errorFlags = "";

                        float widthHeightRatio = chr.PieceWidth;
                        widthHeightRatio /= chr.PieceHeight;

                        if (widthHeightRatio < Configurator.GetDoubleProperty("intelligence_minCharWidthHeightRatio") ||
                            widthHeightRatio > Configurator.GetDoubleProperty("intelligence_maxCharWidthHeightRatio")
                        )
                        {
                            errorFlags += "WHR ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }


                        if ((chr.PositionInPlate.X1 < 2 ||
                             chr.PositionInPlate.X2 > _plate.GetWidth() - 1)
                            && widthHeightRatio < 0.12
                        )
                        {
                            errorFlags += "POS ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }


                        float contrastCost = Math.Abs(chr.StatisticContrast - averageContrast);
                        float brightnessCost = Math.Abs(chr.StatisticAverageBrightness - averageBrightness);
                        float hueCost = Math.Abs(chr.StatisticAverageHue - averageHue);
                        float saturationCost = Math.Abs(chr.StatisticAverageSaturation - averageSaturation);
                        float heightCost = (chr.PieceHeight - averageHeight) / averageHeight;

                        if (brightnessCost > Configurator.GetDoubleProperty("intelligence_maxBrightnessCostDispersion"))
                        {
                            errorFlags += "BRI ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }
                        if (contrastCost > Configurator.GetDoubleProperty("intelligence_maxContrastCostDispersion"))
                        {
                            errorFlags += "CON ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }
                        if (hueCost > Configurator.GetDoubleProperty("intelligence_maxHueCostDispersion"))
                        {
                            errorFlags += "HUE ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }
                        if (saturationCost > Configurator.GetDoubleProperty("intelligence_maxSaturationCostDispersion"))
                        {
                            errorFlags += "SAT ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }
                        if (heightCost < -Configurator.GetDoubleProperty("intelligence_maxHeightCostDispersion"))
                        {
                            errorFlags += "HEI ";
                            ok = false;
                            if (!EnableReportGeneration) continue;
                        }

                        float similarityCost = 0;
                        CharacterRecognizer.RecognizedChar rc = null;
                        if (ok)
                        {
                            rc = ChrRecog.Recognize(chr);
                            similarityCost = rc.GetPatterns()[0].Cost;
                            if (similarityCost >
                                Configurator.GetDoubleProperty("intelligence_maxSimilarityCostDispersion"))
                            {
                                errorFlags += "NEU ";
                                ok = false;
                                if (!EnableReportGeneration) continue;
                            }

                        }

                        if (ok)
                        {
                            recognizedPlate.AddChar(rc);
                        }

                        if (EnableReportGeneration)
                        {
                            Program.ReportGenerator.InsertText("<div class='heuristictable'>");
                            Program.ReportGenerator.InsertImage(
                                Photo.LinearResizeBitmap(chr.GetBitmap(), chr.GetWidth() * 2, chr.GetHeight() * 2),
                                "skeleton", 0, 0);
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>WHR</span><span class='value'>" + widthHeightRatio + "</span>");
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>HEI</span><span class='value'>" + heightCost + "</span>");
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>NEU</span><span class='value'>" + similarityCost + "</span>");
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>CON</span><span class='value'>" + contrastCost + "</span>");
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>BRI</span><span class='value'>" + brightnessCost + "</span>");
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>HUE</span><span class='value'>" + hueCost + "</span>");
                            Program.ReportGenerator.InsertText(
                                "<span class='name'>SAT</span><span class='value'>" + saturationCost + "</span>");
                            Program.ReportGenerator.InsertText("</table>");
                            if (errorFlags.Length != 0)
                                Program.ReportGenerator.InsertText("<span class='errflags'>" + errorFlags + "</span>");
                            Program.ReportGenerator.InsertText("</div>");
                        }
                    }

                    if (recognizedPlate.Chars.Count <
                        Configurator.GetIntProperty("intelligence_minimumChars")) continue;

                    _lastProcessDuration = timeMeter.GetTime();
                    string parsedOutput = Parser.Parse(recognizedPlate, syntaxAnalysisMode);

                    if (EnableReportGeneration)
                    {
                        Program.ReportGenerator.InsertText("<span class='recognized'>");
                        Program.ReportGenerator.InsertText("Recognized _plate : " + parsedOutput);
                        Program.ReportGenerator.InsertText("</span>");
                    }

                    return parsedOutput;

                }
            }

            _lastProcessDuration = timeMeter.GetTime();
            return null;
        }
    }
}
