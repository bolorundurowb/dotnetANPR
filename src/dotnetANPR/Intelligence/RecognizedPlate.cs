using System.Collections.Generic;
using System.Text;
using DotNetANPR.Recognizer;

namespace DotNetANPR.Intelligence;

/// <summary>
/// Container for all recognized characters on a single license plate.
/// Characters are stored in order from left to right as they appear on the plate.
/// </summary>
public class RecognizedPlate
{
    private readonly List<RecognizedCharacter> _characters = new();

    /// <summary>
    /// Gets the list of recognized characters on this plate.
    /// </summary>
    public List<RecognizedCharacter> Characters => _characters;

    /// <summary>
    /// Adds a recognized character to the plate.
    /// </summary>
    /// <param name="character">The recognized character to add.</param>
    public void AddCharacter(RecognizedCharacter character) => _characters.Add(character);

    /// <summary>
    /// Gets the recognized character at the specified position on the plate.
    /// </summary>
    /// <param name="index">The zero-based position index.</param>
    /// <returns>The <see cref="RecognizedCharacter"/> at the given index.</returns>
    public RecognizedCharacter Character(int index) => _characters[index];

    /// <summary>
    /// Returns the plate text by concatenating the top-ranked character from each position.
    /// </summary>
    /// <returns>The recognized plate text string.</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var character in _characters)
            builder.Append(character.Pattern(0)?.Char);

        return builder.ToString();
    }
}
