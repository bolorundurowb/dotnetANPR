using System.IO;
using dotNETANPR.Gui;
using dotNETANPR.ImageAnalysis;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class Intelligence
    {
        private long lastProcessDuration = 0;
        public static Configurator.Configurator Configurator { get; set; } = new Configurator.Configurator(
            "." +
            Path.DirectorySeparatorChar +
            "config.xml"
        );

        public static CharacterRecognizer chrRecog;
        public static Parser parser;
        public bool enableReportGeneration;

        public Intelligence(bool enableReportGeneration)
        {
            this.enableReportGeneration = enableReportGeneration;
            int classificationMethod = Configurator.GetIntProperty("intelligence_classification_method");
            if (classificationMethod == 0)
            {
                chrRecog = new KnnPatterClassificator();
            }
            else
            {
                chrRecog = new NeuralPatternClassificator();
            }
            parser = new Parser();
        }

        public long LastProcessDuration()
        {
            return lastProcessDuration;
        }

        public string Recognize(CarSnapshot carSnapshot)
        {
            TimeMeter timeMeter = new TimeMeter();
            int syntaxAnalysisMode = Configurator.GetIntProperty("intelligence_syntaxanalysis");
            int skewDetectionMode = Configurator.GetIntProperty("intelligence_skewdetection");
            
            if (enableReportGeneration) {
                Program.ReportGenerator.InsertText("<h1>Automatic Number Plate Recognition Report</h1>");
                Program.ReportGenerator.InsertText("<span>Image width: "+carSnapshot.GetWidth()+" px</span>");
                Program.ReportGenerator.InsertText("<span>Image height: "+carSnapshot.GetHeight()+" px</span>");
            
                Program.ReportGenerator.InsertText("<h2>Vertical and Horizontal plate projection</h2>");
            
                Program.ReportGenerator.InsertImage(carSnapshot.RenderGraph(), "snapshotgraph",0,0);
                Program.ReportGenerator.InsertImage(carSnapshot.GetBitmapWithAxes(), "snapshot",0,0);
            }

            foreach (Band band in carSnapshot.GetBands())
            {

            }

            this.lastProcessDuration = timeMeter.GetTime();
            return null;
        }
    }
}
