namespace DotNetANPR.Recognizer;

public class RecognizedPattern(char character, double similarity)
{
    public char Character { get; } = character;
    public double Similarity { get; } = similarity;
}