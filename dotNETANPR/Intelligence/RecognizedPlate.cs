using System;
using System.Collections.Generic;
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
            string ret = String.Empty;
            for (int i = 0; i < _chars.Count; i++)
            {
                ret += _chars[i].GetPattern(0).Character;
            }
            return ret;
        }
    }
}
