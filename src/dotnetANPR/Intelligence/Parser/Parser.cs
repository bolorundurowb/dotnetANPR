using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using dotnetANPR.Intelligence;
using Microsoft.Extensions.Logging;

namespace dotnetANPR.Intelligence.Parser;

internal sealed class Parser
{
    private readonly ILogger _logger;
    private readonly List<PlateForm> _plateForms;

    public Parser(string syntaxFilePath, ILogger logger)
    {
        _logger = logger;
        _plateForms = [];

        if (string.IsNullOrEmpty(syntaxFilePath))
            throw new IOException("Failed to get syntax description file path");

        try
        {
            _plateForms = LoadFromXml(syntaxFilePath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load parser syntax description file");
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
            foreach (XmlNode charNode in typeNode.ChildNodes)
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
        foreach (var form in _plateForms)
            form.IsFlagged = false;
    }

    public void FlagEqualOrShorterLength(int length)
    {
        var found = false;
        for (var i = length; i >= 1 && !found; i--)
            foreach (var form in _plateForms.Where(form => form.Length == i))
            {
                form.IsFlagged = true;
                found = true;
            }
    }

    public void FlagEqualLength(int length)
    {
        foreach (var form in _plateForms)
            if (form.Length == length)
                form.IsFlagged = true;
    }

    public string Parse(RecognizedPlate recognizedPlate, SyntaxAnalysisMode syntaxAnalysisMode)
    {
        var length = recognizedPlate.Characters.Count;

        switch (syntaxAnalysisMode)
        {
            case SyntaxAnalysisMode.DoNotParse:
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

        List<FinalPlate> finalPlates = [];

        foreach (var form in _plateForms)
        {
            if (!form.IsFlagged)
                continue;

            for (var i = 0; i <= length - form.Length; i++)
            {
                _logger.LogDebug("Comparing plate with form {FormName} and offset {Offset}", form.Name, i);
                var finalPlate = new FinalPlate();
                for (var j = 0; j < form.Length; j++)
                {
                    var rc = recognizedPlate.Characters[j + i];
                    if (form.Positions[j].IsAllowed(rc.Patterns![0].Char))
                    {
                        finalPlate.AddChar(rc.Patterns[0].Char);
                    }
                    else
                    {
                        finalPlate.RequiredChanges++;
                        foreach (var rp in rc.Patterns.Where(t => form.Positions[j].IsAllowed(t.Char)))
                        {
                            finalPlate.RequiredChanges += rp.Cost / 100.0;
                            finalPlate.AddChar(rp.Char);
                            break;
                        }
                    }
                }

                finalPlates.Add(finalPlate);
            }
        }

        if (finalPlates.Count == 0)
            return recognizedPlate.ToString();

        var minimalChanges = double.PositiveInfinity;
        var minimalIndex = 0;
        for (var i = 0; i < finalPlates.Count; i++)
        {
            if (finalPlates[i].RequiredChanges <= minimalChanges)
            {
                minimalChanges = finalPlates[i].RequiredChanges;
                minimalIndex = i;
            }
        }

        var toReturn = recognizedPlate.ToString();
        if (finalPlates[minimalIndex].RequiredChanges <= 2)
            toReturn = finalPlates[minimalIndex].Plate;

        return toReturn;
    }

    private sealed class FinalPlate
    {
        public string Plate { get; private set; } = string.Empty;
        public double RequiredChanges { get; set; }
        public void AddChar(char chr) => Plate += chr;
    }
}
