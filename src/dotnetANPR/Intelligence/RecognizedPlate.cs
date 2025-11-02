namespace DotNetANPR.Intelligence
{
    public class RecognizedPlate
    {
        public string Text { get; }
        public double Confidence { get; }
        public ImageAnalysis.LicensePlate SourcePlate { get; }

        public RecognizedPlate(string text, double confidence, ImageAnalysis.LicensePlate plate)
        {
            Text = text;
            Confidence = confidence;
            SourcePlate = plate;
        }
    }
}