using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.Recognizer;

public class RecognizedCharacter
{
    private List<RecognizedPattern> _patterns;

    public bool IsSorted { get; private set; }

    public List<RecognizedPattern>? Patterns => !IsSorted ? null : _patterns;

    public RecognizedCharacter()
    {
        IsSorted = false;
        _patterns = new List<RecognizedPattern>();
    }

    public RecognizedPattern? Pattern(int index) => Patterns?.ElementAtOrDefault(index);

    public void Sort()
    {
        if (IsSorted) 
            return;

        _patterns.Sort();
        IsSorted = true;
    }
}
