using System;
using System.Collections.Generic;
using System.Linq;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class RecognizedPlate
    {
        private readonly List<CharacterRecognizer.RecognizedChar> _chars;

        public RecognizedPlate()
        {
            _chars = new List<CharacterRecognizer.RecognizedChar>();
        }

        public void AddChar(CharacterRecognizer.RecognizedChar chr)
        {
            _chars.Add(chr);
        }

        public CharacterRecognizer.RecognizedChar GetChar(int i)
        {
            return _chars[i];
        }

        public string GetString()
        {
            return _chars.Aggregate(string.Empty, (current, t) => current + t.GetPattern(0).Character);
        }
    }
}
