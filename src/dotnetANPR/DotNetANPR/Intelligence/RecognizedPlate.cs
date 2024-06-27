using System.Collections.Generic;
using System.Text;
using DotNetANPR.Recognizer;

namespace DotNetANPR.Intelligence;

public class RecognizedPlate
{
    private readonly List<RecognizedCharacter> _characters = new();

    public void AddCharacter(RecognizedCharacter character) => _characters.Add(character);

    public RecognizedCharacter Character(int index) => _characters[index];

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var character in _characters)
            builder.Append(character.Pattern(0)?.Char);

        return builder.ToString();
    }
}