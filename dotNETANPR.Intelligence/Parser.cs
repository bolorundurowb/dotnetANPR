using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using dotNETANPR.Configurator;
using dotNETANPR.Recognizer;

namespace dotNETANPR.Intelligence
{
    public class Parser
    {
        public class PlateForm
        {
            public class Position
            {
                public char[] allowedChars;
                public Position(String data)
                {
                    this.allowedChars = data.ToCharArray();
                }
                public bool isAllowed(char chr)
                {
                    bool ret = false;
                    for (int i = 0; i < this.allowedChars.Length; i++)
                        if (this.allowedChars[i] == chr)
                            ret = true;
                    return ret;
                }
            }
            List<Position> positions;
            String name;
            public bool flagged = false;

            public PlateForm(String name)
            {
                this.name = name;
                this.positions = new List<Position>();
            }
            public void addPosition(Position p)
            {
                this.positions.Add(p);
            }
            public Position getPosition(int index)
            {
                return this.positions.ElementAt(index);
            }
            public int length()
            {
                return this.positions.Count;
            }

        }
        public class FinalPlate
        {
            public String plate;
            public float requiredChanges = 0;
            public FinalPlate()
            {
                this.plate = "";
            }
            public void addChar(char chr)
            {
                this.plate = this.plate + chr;
            }
        }

        List<PlateForm> plateForms;

        /** Creates a new instance of Parser */
        public Parser()
        {
            this.plateForms = new List<PlateForm>();
            Configurator.Configurator config = new Configurator.Configurator();
            this.plateForms = this.loadFromXml(config.getPathProperty("intelligence_syntaxDescriptionFile"));
        }

        public List<PlateForm> loadFromXml(String fileName)
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
                    String content = ((XmlElement)charNode).GetAttribute("content");

                    form.addPosition(new Parser.PlateForm.Position(  content.ToUpper() ));
                }
                plateForms.Add(form);
            }
            return plateForms;
        }
        ////
        public void unFlagAll()
        {
            foreach (PlateForm form in this.plateForms)
                form.flagged = false;
        }
        // pre danu dlzku znacky sa pokusi najst nejaky plateform o rovnakej dlzke
        // v pripade ze nenajde ziadny, pripusti moznost ze je nejaky znak navyse
        // a hlada plateform s mensim poctom pismen
        public void flagEqualOrShorterLength(int length)
        {
            bool found = false;
            for (int i = length; i >= 1 && !found; i--)
            {
                foreach (PlateForm form in this.plateForms)
                {
                    if (form.length() == i)
                    {
                        form.flagged = true;
                        found = true;
                    }
                }
            }
        }

        public void flagEqualLength(int length)
        {
            foreach (PlateForm form in this.plateForms)
            {
                if (form.length() == length)
                {
                    form.flagged = true;
                }
            }
        }

        public void invertFlags()
        {
            foreach (PlateForm form in this.plateForms)
                form.flagged = !form.flagged;
        }

        // syntax analysis mode : 0 (do not parse)
        //                      : 1 (only equal length)
        //                      : 2 (equal or shorter)
        public String parse(RecognizedPlate recognizedPlate, int syntaxAnalysisMode)
        {
            if (syntaxAnalysisMode == 0)
            {
                Main.rg.insertText(" result : "+recognizedPlate.getString()+" --> <font size=15>"+recognizedPlate.getString()+"</font><hr><br>");
                return recognizedPlate.getString();
            }

            int length = recognizedPlate.chars.Count;
            this.unFlagAll();
            if (syntaxAnalysisMode == 1)
            {
                this.flagEqualLength(length);
            }
            else
            {
                this.flagEqualOrShorterLength(length);
            }

            List<FinalPlate> finalPlates = new List<FinalPlate>();

            foreach (PlateForm form in this.plateForms)
            {
                if (!form.flagged) continue; // skip unflagged
                for (int i = 0; i <= length - form.length(); i++)
                { // posuvanie formy po znacke
                    //                System.out.println("comparing "+recognizedPlate.getString()+" with form "+form.name+" and offset "+i );
                    FinalPlate finalPlate = new FinalPlate();
                    for (int ii = 0; ii < form.length(); ii++)
                    { // prebehnut vsetky znaky formy
                        // form.getPosition(ii).allowedChars // zoznam povolenych
                        CharacterRecognizer.RecognizedChar rc = recognizedPlate.getChar(ii + i); // znak na znacke

                        if (form.getPosition(ii).isAllowed(rc.getPattern(0).getChar))
                        {
                            finalPlate.addChar(rc.getPattern(0).getChar);
                        }
                        else
                        { // treba vymenu
                            finalPlate.requiredChanges++; // +1 za pismeno
                            for (int x = 0; x < rc.getPatterns().Count; x++)
                            {
                                if (form.getPosition(ii).isAllowed(rc.getPattern(x).getChar))
                                {
                                    CharacterRecognizer.RecognizedChar.RecognizedPattern rp = rc.getPattern(x);
                                    finalPlate.requiredChanges += (rp.getCost / 100);  // +x za jeho cost
                                    finalPlate.addChar(rp.getChar);
                                    break;
                                }
                            }
                        }
                    }
                    //                System.out.println("adding "+finalPlate.plate+" with required changes "+finalPlate.requiredChanges);
                    finalPlates.Add(finalPlate);
                }
            }
            //        



            // tu este osetrit nespracovanie znacky v pripade ze nebola oznacena ziadna
            if (finalPlates.Count == 0) return recognizedPlate.getString();
            // else :
            // najst tu s najmensim poctom vymen
            float minimalChanges = float.PositiveInfinity;
            int minimalIndex = 0;
            //        System.out.println("---");
            for (int i = 0; i < finalPlates.Count; i++)
            {
                //            System.out.println("::"+finalPlates.ElementAt(i).plate+" "+finalPlates.ElementAt(i).requiredChanges);
                if (finalPlates.ElementAt(i).requiredChanges <= minimalChanges)
                {
                    minimalChanges = finalPlates.ElementAt(i).requiredChanges;
                    minimalIndex = i;
                }
            }

            String toReturn = recognizedPlate.getString();
            if (finalPlates.ElementAt(minimalIndex).requiredChanges <= 2)
                toReturn = finalPlates.ElementAt(minimalIndex).plate;
            return toReturn;
        }
    }
}