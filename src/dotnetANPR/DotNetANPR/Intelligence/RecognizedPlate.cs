using System.Collections.Generic;
using System.Text;
using DotNetANPR.Recognizer;

namespace DotNetANPR.Intelligence
{
    public class RecognizedPlate
    {
        private readonly List<RecognizedCharacter> _characters = new List<RecognizedCharacter>();

        public void AddCharacter(RecognizedCharacter character) => _characters.Add(character);

        public RecognizedCharacter GetCharacter(int index) => _characters[index];

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var character in _characters)
                builder.Append(character.Pattern.Character);

            return builder.ToString();
        }
    }
}
