using dotNETANPR.Recognizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.Intelligence
{
    class RecognizedPlate
    {
        public List<CharacterRecognizer.RecognizedChar> chars;

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
            return chars.ElementAt(i);
        }

        public string GetString()
        {
            string ret = "";
            for (int i = 0; i < chars.Count; i++)
            {

                ret = ret + chars.ElementAt(i).GetPattern(0).GetChar;
            }
            return ret;
        }
    }
}
