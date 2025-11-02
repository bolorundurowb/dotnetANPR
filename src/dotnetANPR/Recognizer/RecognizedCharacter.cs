using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.Recognizer
{
    public class RecognizedChar
    {
        public List<RecognizedPattern> Patterns { get; }
        public bool IsSorted { get; private set; }

        public RecognizedChar()
        {
            Patterns = new List<RecognizedPattern>();
            IsSorted = false;
        }

        public void AddPattern(RecognizedPattern pattern)
        {
            Patterns.Add(pattern);
            IsSorted = false;
        }

        public void Sort()
        {
            if (IsSorted) return;
            Patterns.Sort((p1, p2) => p1.Similarity.CompareTo(p2.Similarity));
            IsSorted = true;
        }

        public char GetBestPattern()
        {
            Sort();
            return Patterns.Any() ? Patterns[0].Character : '?';
        }
    }
}