using System.Linq;

namespace DotNetANPR.Intelligence.Parser
{
    public class Position
    {
        public char[] AllowedChars { get; private set; }

        public Position(string data) => AllowedChars = data.ToCharArray();

        public bool IsAllowed(char character) => AllowedChars.Contains(character);
    }
}
