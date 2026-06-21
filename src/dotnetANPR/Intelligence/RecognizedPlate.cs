using System.Collections.Generic;
using System.Text;
using dotnetANPR.Recognizer;

namespace dotnetANPR.Intelligence;

/// <summary>
/// Holds the collection of recognised characters that form a licence plate result.
/// </summary>
public class RecognizedPlate
{
    private readonly List<RecognizedCharacter> _characters = [];

    /// <summary>
    /// Gets the list of recognised characters in this plate.
    /// </summary>
    public List<RecognizedCharacter> Characters => _characters;

    /// <summary>
    /// Adds a recognised character to the plate.
    /// </summary>
    public void AddCharacter(RecognizedCharacter character) => _characters.Add(character);

    /// <summary>
    /// Returns the recognised character at the specified index.
    /// </summary>
    public RecognizedCharacter Character(int index) => _characters[index];

    /// <summary>
    /// Returns the full plate text by concatenating the best-match character from each position.
    /// </summary>
    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var character in _characters)
            builder.Append(character.Pattern(0)?.Char);

        return builder.ToString();
    }
}