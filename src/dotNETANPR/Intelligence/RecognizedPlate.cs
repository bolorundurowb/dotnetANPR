using System;
using System.Collections.Generic;
using System.Linq;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class RecognizedPlate
    {
        public readonly List<CharacterRecognizer.RecognizedChar> Chars;

        public RecognizedPlate()
        {
            Chars = new List<CharacterRecognizer.RecognizedChar>();
        }

        public void AddChar(CharacterRecognizer.RecognizedChar chr)
        {
            Chars.Add(chr);
        }

        public CharacterRecognizer.RecognizedChar GetChar(int i)
        {
            return Chars[i];
        }

        public string GetString()
        {
            return Chars.Aggregate(string.Empty, (current, t) => current + t.GetPattern(0).Character);
        }
    }
}
