using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using DotNetANPR.Configuration;
using DotNetANPR.Recognizer;
using Microsoft.Extensions.Logging;

namespace DotNetANPR.Intelligence.Parser;

public class Parser
{
    private static readonly ILogger<Parser> logger =
        LoggerFactory.Create(_ => { }).CreateLogger<Parser>();

    private List<PlateForm> plateForms;

    public Parser()
    {
        plateForms = new();
        var fileName = Configurator.Instance.GetPath("intelligence_syntaxDescriptionFile");
        if (string.IsNullOrEmpty(fileName))
        {
            throw new IOException("Failed to get syntax description file from Configurator");
        }

        try
        {
            plateForms = LoadFromXml(fileName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load from parser syntax description file");
            throw;
        }
    }

    public List<PlateForm> LoadFromXml(string fileName)
    {
        var plateForms = new List<PlateForm>();
        var doc = new XmlDocument();
        doc.Load(fileName);

        var structureNode = doc.DocumentElement;
        var structureNodeContent = structureNode?.ChildNodes;

        if (structureNodeContent is null)
            throw new IOException("Failed to load from parser syntax description file");

        foreach (XmlNode typeNode in structureNodeContent)
        {
            if (typeNode.Name != "type")
                continue;

            var form = new PlateForm(((XmlElement)typeNode).GetAttribute("name"));
            var typeNodeContent = typeNode.ChildNodes;

            foreach (XmlNode charNode in typeNodeContent)
            {
                if (charNode.Name != "char")
                    continue;

                var content = ((XmlElement)charNode).GetAttribute("content");
                form.Positions.Add(new Position(content.ToUpper()));
            }

            plateForms.Add(form);
        }

        return plateForms;
    }

    public void UnFlagAll()
    {
        foreach (var form in plateForms)
            form.IsFlagged = false;
    }

    /**
     * For given {@code length}, finds a {@link PlateForm} of the same length. If
     * no such {@link PlateForm} is found, tries to find one with less characters.
     *
     * @param length the number of characters of the PlateForm
     */
    public void FlagEqualOrShorterLength(int length)
    {
        var found = false;
        for (var i = length; (i >= 1) && !found; i--)
            foreach (var form in plateForms)
                if (form.Length == i)
                {
                    form.IsFlagged = true;
                    found = true;
                }
    }

    public void FlagEqualLength(int length)
    {
        foreach (var form in plateForms)
            if (form.Length == length)
                form.IsFlagged = true;
    }

    public void InvertFlags()
    {
        foreach (var form in plateForms)
            form.IsFlagged = (!form.IsFlagged);
    }

    /**
     * Syntactically parses text from the given {@link RecognizedPlate} in the specified analysis mode.
     * <p>
     *
     * @param recognizedPlate the plate to parse
     * @param syntaxAnalysisMode the mode in which to parse
     * @return the parsed recognized plate text
     */
    public string Parse(RecognizedPlate recognizedPlate, SyntaxAnalysisMode syntaxAnalysisMode)
    {
        var length = recognizedPlate.Characters.Count;

        switch (syntaxAnalysisMode)
        {
            case SyntaxAnalysisMode.DoNotParse:
                // TODO: figure out how to avoid this
                // Main.rg.insertText(
                //     $" result : {recognizedPlate} --> <font size=15>{recognizedPlate}</font><hr><br>");
                return recognizedPlate.ToString();
            case SyntaxAnalysisMode.OnlyEqualLength:
                UnFlagAll();
                FlagEqualLength(length);
                break;
            case SyntaxAnalysisMode.EqualOrShorterLength:
                UnFlagAll();
                FlagEqualOrShorterLength(length);
                break;
            default:
                throw new ArgumentException("Unknown syntax analysis mode: " + syntaxAnalysisMode);
        }

        List<FinalPlate> finalPlates = new();

        foreach (var form in plateForms)
        {
            if (!form.IsFlagged)
                continue;

            for (var i = 0; i <= (length - form.Length); i++)
            {
                // moving the form on the plate
                logger.LogDebug("Comparing {} with form {} and offset {}.", recognizedPlate, form.Name, i);
                var finalPlate = new FinalPlate();
                for (var j = 0; j < form.Length; j++)
                {
                    // all chars of the form
                    var rc = recognizedPlate.Characters[j + i];
                    if (form.Positions[j].IsAllowed(rc.Patterns![0].Char))
                    {
                        finalPlate.AddChar(rc.Patterns[0].Char);
                    }
                    else
                    {
                        // a swap needed
                        finalPlate.RequiredChanges++; // +1 for every char
                        foreach (var rp in rc.Patterns.Where(t => form.Positions[j].IsAllowed(t.Char)))
                        {
                            finalPlate.RequiredChanges += (rp.Cost / 100.0); // +x for its cost
                            finalPlate.AddChar(rp.Char);
                            break;
                        }
                    }
                }

                logger.LogDebug("Adding {} with required changes {}.", finalPlate.Plate, finalPlate.RequiredChanges);
                finalPlates.Add(finalPlate);
            }
        }

        if (finalPlates.Count == 0)
        {
            return recognizedPlate.ToString();
        }

        // else: find the plate with the lowest number of swaps
        var minimalChanges = double.PositiveInfinity;
        var minimalIndex = 0;
        for (var i = 0; i < finalPlates.Count; i++)
        {
            logger.LogDebug("Plate {} : {} with required changes {}.", i, finalPlates[i].Plate,
                finalPlates[i].RequiredChanges);
            if (finalPlates[i].RequiredChanges <= minimalChanges)
            {
                minimalChanges = finalPlates[i].RequiredChanges;
                minimalIndex = i;
            }
        }

        var toReturn = recognizedPlate.ToString();
        if (finalPlates[minimalIndex].RequiredChanges <= 2)
        {
            toReturn = finalPlates[minimalIndex].Plate;
        }

        return toReturn;
    }

    #region Private Helpers

    private class FinalPlate
    {
        public string Plate { get; private set; } = string.Empty;

        public double RequiredChanges { get; internal set; } = 0;

        public void AddChar(char chr) { Plate += chr; }
    }

    #endregion
}
