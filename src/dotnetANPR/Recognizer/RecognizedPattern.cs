namespace DotNetANPR.Recognizer
{
    public class RecognizedPattern
    {
        public char Character { get; }
        public double Similarity { get; }

        public RecognizedPattern(char character, double similarity)
        {
            Character = character;
            Similarity = similarity;
        }
    }
}