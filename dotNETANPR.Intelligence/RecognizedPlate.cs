using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class RecognizedPlate
    {
        public List<CharacterRecognizer.RecognizedChar> chars;

        public RecognizedPlate()
        {
            this.chars = new List<CharacterRecognizer.RecognizedChar>();
        }

        public void addChar(CharacterRecognizer.RecognizedChar chr)
        {
            this.chars.Add(chr);
        }

        public CharacterRecognizer.RecognizedChar getChar(int i)
        {
            return this.chars.ElementAt(i);
        }

        public String getString()
        {
            string ret = "";
            for (int i = 0; i < chars.Count; i++)
            {

                ret = ret + this.chars.ElementAt(i).getPattern(0).getChar;
            }
            return ret;
        }

    }
}
