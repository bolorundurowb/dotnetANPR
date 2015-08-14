using dotNETANPR.Recognizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dotNETANPR.Intelligence
{
    class Parser
    {
        public class PlateForm
        {
            public class Position
            {
                public char[] allowedChars;
                public Position(string data)
                {
                    allowedChars = data.ToCharArray();
                }
                public bool isAllowed(char chr)
                {
                    bool ret = false;
                    for (int i = 0; i < allowedChars.Length; i++)
                        if (allowedChars[i] == chr)
                            ret = true;
                    return ret;
                }
            }
            List<Position> positions;
            string name;
            public bool flagged = false;

            public PlateForm(string name)
            {
                this.name = name;
                positions = new List<Position>();
            }

            public void AddPosition(Position p)
            {
                positions.Add(p);
            }

            public Position GetPosition(int index)
            {
                return positions.ElementAt(index);
            }

            public int Length
            {
                get
                {
                    return positions.Count;
                }
            }

        }
        public class FinalPlate
        {
            public string plate;
            public float requiredChanges = 0;
            public FinalPlate()
            {
                plate = "";
            }
            public void addChar(char chr)
            {
                plate = plate + chr;
            }
        }

        List<PlateForm> plateForms;
        
        public Parser()
        {
            plateForms = new List<PlateForm>();
            Configurator.Configurator config = new Configurator.Configurator();
            plateForms = loadFromXml(config.GetPathProperty("intelligence_syntaxDescriptionFile"));
        }

        public List<PlateForm> loadFromXml(string fileName)
        {
            List<PlateForm> plateForms = new List<PlateForm>();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode structureNode = doc.DocumentElement;
            XmlNodeList structureNodeContent = structureNode.ChildNodes;
            for (int i = 0; i < structureNodeContent.Count; i++)
            {
                XmlNode typeNode = structureNodeContent.Item(i);
                if (!typeNode.Name.Equals("type")) continue;
                PlateForm form = new PlateForm(((XmlElement)typeNode).GetAttribute("name"));
                XmlNodeList typeNodeContent = typeNode.ChildNodes;
                for (int ii = 0; ii < typeNodeContent.Count; ii++)
                {
                    XmlNode charNode = typeNodeContent.Item(ii);
                    if (!charNode.Name.Equals("char")) continue;
                    string content = ((XmlElement)charNode).GetAttribute("content");

                    form.AddPosition(new Parser.PlateForm.Position(content.ToUpper()));
                }
                plateForms.Add(form);
            }
            return plateForms;
        }
        
        public void UnFlagAll()
        {
            foreach (PlateForm form in plateForms)
                form.flagged = false;
        }

        public void FlagEqualOrShorterLength(int Length)
        {
            bool found = false;
            for (int i = Length; i >= 1 && !found; i--)
            {
                foreach (PlateForm form in plateForms)
                {
                    if (form.Length == i)
                    {
                        form.flagged = true;
                        found = true;
                    }
                }
            }
        }

        public void FlagEqualLength(int Length)
        {
            foreach (PlateForm form in plateForms)
            {
                if (form.Length == Length)
                {
                    form.flagged = true;
                }
            }
        }

        public void invertFlags()
        {
            foreach (PlateForm form in plateForms)
                form.flagged = !form.flagged;
        }

        // syntax analysis mode : 0 (do not parse)
        //                      : 1 (only equal Length)
        //                      : 2 (equal or shorter)
        public string parse(RecognizedPlate recognizedPlate, int syntaxAnalysisMode)
        {
            if (syntaxAnalysisMode == 0)
            {
                CMain.rg.InsertText(" result : " + recognizedPlate.GetString() + " --> <font size=15>" + recognizedPlate.GetString() + "</font><hr><br>");
                return recognizedPlate.GetString();
            }

            int Length = recognizedPlate.chars.Count;
            UnFlagAll();
            if (syntaxAnalysisMode == 1)
            {
                FlagEqualLength(Length);
            }
            else
            {
                FlagEqualOrShorterLength(Length);
            }

            List<FinalPlate> finalPlates = new List<FinalPlate>();

            foreach (PlateForm form in plateForms)
            {
                if (!form.flagged) continue; 
                for (int i = 0; i <= Length - form.Length; i++)
                { 
                    FinalPlate finalPlate = new FinalPlate();
                    for (int ii = 0; ii < form.Length; ii++)
                    { 
                        CharacterRecognizer.RecognizedChar rc = recognizedPlate.GetChar(ii + i);

                        if (form.GetPosition(ii).isAllowed(rc.GetPattern(0).GetChar))
                        {
                            finalPlate.addChar(rc.GetPattern(0).GetChar);
                        }
                        else
                        { 
                            finalPlate.requiredChanges++; 
                            for (int x = 0; x < rc.GetPatterns().Count; x++)
                            {
                                if (form.GetPosition(ii).isAllowed(rc.GetPattern(x).GetChar))
                                {
                                    CharacterRecognizer.RecognizedChar.RecognizedPattern rp = rc.GetPattern(x);
                                    finalPlate.requiredChanges += (rp.GetCost / 100); 
                                    finalPlate.addChar(rp.GetChar);
                                    break;
                                }
                            }
                        }
                    }
                    finalPlates.Add(finalPlate);
                }
            }
            if (finalPlates.Count == 0) return recognizedPlate.GetString();
            float minimalChanges = float.PositiveInfinity;
            int minimalIndex = 0;
            for (int i = 0; i < finalPlates.Count; i++)
            {
                if (finalPlates.ElementAt(i).requiredChanges <= minimalChanges)
                {
                    minimalChanges = finalPlates.ElementAt(i).requiredChanges;
                    minimalIndex = i;
                }
            }

            string toReturn = recognizedPlate.GetString();
            if (finalPlates.ElementAt(minimalIndex).requiredChanges <= 2)
                toReturn = finalPlates.ElementAt(minimalIndex).plate;
            return toReturn;
        }
    }
}
