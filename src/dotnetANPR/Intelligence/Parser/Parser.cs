using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DotNetANPR.Configuration;
namespace DotNetANPR.Intelligence.Parser;

/// <summary>
/// Loads plate syntax templates from an XML description file and matches recognized plates
/// against those templates to produce a best-fit parsed result.
/// </summary>
public class Parser
{
    private readonly List<PlateForm> _plateForms;

    /// <summary>
    /// Initializes a new <see cref="Parser"/> by loading syntax templates from the
    /// configured XML description file.
    /// </summary>
    /// <exception cref="IOException">
    /// Thrown when the syntax description file path is not configured or cannot be loaded.
    /// </exception>
    public Parser()
    {
        _plateForms = new List<PlateForm>();
        var fileName = AnprConfig.Instance.Intelligence.SyntaxDescriptionFile
            .Replace('/', System.IO.Path.DirectorySeparatorChar)
            .Replace('\\', System.IO.Path.DirectorySeparatorChar);

        if (string.IsNullOrEmpty(fileName))
            throw new IOException("Failed to get syntax description file from AnprConfig");

        try
        {
            _plateForms = LoadFromXml(fileName);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Loads plate format templates from the specified XML file.
    /// </summary>
    /// <param name="fileName">The path to the syntax XML file.</param>
    /// <returns>A list of parsed <see cref="PlateForm"/> templates.</returns>
    public List<PlateForm> LoadFromXml(string fileName)
    {
        var plateForms = new List<PlateForm>();
        var doc = XDocument.Load(fileName);
        var root = doc.Root;

        if (root is null)
            throw new IOException("Failed to load from parser syntax description file: no root element");

        foreach (var typeNode in root.Elements("type"))
        {
            var name = typeNode.Attribute("name")?.Value ?? string.Empty;
            var form = new PlateForm(name);

            foreach (var charNode in typeNode.Elements("char"))
            {
                var content = charNode.Attribute("content")?.Value ?? string.Empty;
                form.Positions.Add(new Position(content.ToUpper()));
            }

            plateForms.Add(form);
        }

        return plateForms;
    }

    /// <summary>
    /// Clears the flagged state on all plate form templates.
    /// </summary>
    public void UnFlagAll()
    {
        foreach (var form in _plateForms)
            form.IsFlagged = false;
    }

    /// <summary>
    /// Flags all plate forms whose length equals or is shorter than the specified length.
    /// Searches from the given length downward and stops once at least one match is found.
    /// </summary>
    /// <param name="length">The maximum length to match.</param>
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

    /// <summary>
    /// Flags all plate forms whose length exactly matches the specified length.
    /// </summary>
    /// <param name="length">The length to match.</param>
    public void FlagEqualLength(int length)
    {
        foreach (var form in _plateForms)
            if (form.Length == length)
                form.IsFlagged = true;
    }

    /// <summary>
    /// Inverts the flagged state of all plate form templates.
    /// </summary>
    public void InvertFlags()
    {
        foreach (var form in _plateForms)
            form.IsFlagged = !form.IsFlagged;
    }

    /// <summary>
    /// Parses the recognized plate against all flagged syntax templates and returns
    /// the best-matching plate string.
    /// </summary>
    /// <param name="recognizedPlate">The plate containing recognized characters.</param>
    /// <param name="syntaxAnalysisMode">The analysis mode controlling template selection.</param>
    /// <returns>The parsed plate string, potentially corrected by syntax matching.</returns>
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

        var finalPlates = new List<FinalPlate>();

        foreach (var form in _plateForms)
        {
            if (!form.IsFlagged)
                continue;

            for (var i = 0; i <= length - form.Length; i++)
            {
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
                        // A character swap is needed
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

        // Find the plate with the fewest required changes
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

    #region Private Helpers

    private class FinalPlate
    {
        public string Plate { get; private set; } = string.Empty;

        public double RequiredChanges { get; internal set; }

        public void AddChar(char chr) => Plate += chr;
    }

    #endregion
}
