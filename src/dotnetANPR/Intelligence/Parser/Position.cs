using System.Linq;

namespace DotNetANPR.Intelligence.Parser;

/// <summary>
/// Represents a single position in a plate format template, defining which characters
/// are allowed at that position.
/// </summary>
/// <param name="data">
/// A string whose individual characters define the set of allowed characters for this position.
/// </param>
public class Position(string data)
{
    /// <summary>
    /// Gets the array of characters that are allowed at this plate position.
    /// </summary>
    public char[] AllowedChars { get; } = data.ToCharArray();

    /// <summary>
    /// Determines whether the specified character is allowed at this position.
    /// </summary>
    /// <param name="chr">The character to test.</param>
    /// <returns><c>true</c> if the character is in the allowed set; otherwise <c>false</c>.</returns>
    public bool IsAllowed(char chr) => AllowedChars.Contains(chr);
}
