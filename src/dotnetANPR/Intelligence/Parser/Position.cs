using System.Linq;

namespace dotnetANPR.Intelligence.Parser;

public class Position(string data)
{
    public char[] AllowedChars { get; private set; } = data.ToCharArray();

    public bool IsAllowed(char chr) => AllowedChars.Contains(chr);
}