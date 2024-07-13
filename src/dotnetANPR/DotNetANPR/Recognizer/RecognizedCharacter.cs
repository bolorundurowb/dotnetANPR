using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.Recognizer;

public class RecognizedCharacter
{
    private List<RecognizedPattern> _patterns = new();

    public bool IsSorted { get; private set; } = false;

    public List<RecognizedPattern>? Patterns => !IsSorted ? null : _patterns;

    public RecognizedPattern? Pattern(int index) => Patterns?.ElementAtOrDefault(index);

    public void Sort()
    {
        if (IsSorted) 
            return;

        _patterns.Sort();
        IsSorted = true;
    }
}
