namespace DotNetANPR.Intelligence;

public class RecognizedPlate(string text, double confidence, ImageAnalysis.LicensePlate plate)
{
    public string Text { get; } = text;
    public double Confidence { get; } = confidence;
    public ImageAnalysis.LicensePlate SourcePlate { get; } = plate;
}