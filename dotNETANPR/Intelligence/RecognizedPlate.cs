using System;
using System.Collections.Generic;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class RecognizedPlate
    {
        private List<CharacterRecognizer.RecognizedChar> chars;

        public RecognizedPlate()
        {
            chars = new List<CharacterRecognizer.RecognizedChar>();
        }

        public void AddChar(CharacterRecognizer.RecognizedChar chr)
        {
            chars.Add(chr);
        }

        public CharacterRecognizer.RecognizedChar GetChar(int i)
        {
            return chars[i];
        }

        public string GetString()
        {
            string ret = String.Empty;
            for (int i = 0; i < chars.Count; i++)
            {
                ret += chars[i].GetPattern(0).Character;
            }
            return ret;
        }
    }
}
