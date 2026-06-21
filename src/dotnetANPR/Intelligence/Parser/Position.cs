using System.Linq;

namespace dotnetANPR.Intelligence.Parser;

/// <summary>
/// Defines the allowed character set for a single position in a <see cref="PlateForm"/> template.
/// </summary>
/// <param name="data">A string containing all characters allowed at this position.</param>
public class Position(string data)
{
    /// <summary>The set of characters allowed at this position.</summary>
    public char[] AllowedChars { get; private set; } = data.ToCharArray();

    /// <summary>
    /// Checks whether the given character is allowed at this position.
    /// </summary>
    public bool IsAllowed(char chr) => AllowedChars.Contains(chr);
}