namespace DotNetANPR.Recognizer
{
    public class RecognizedPattern
    {
        public char Char { get; private set; }

        public double Cost { get; private set; }

        public RecognizedPattern(char c, double cost)
        {
            Char = c;
            Cost = cost;
        }
    }
}
