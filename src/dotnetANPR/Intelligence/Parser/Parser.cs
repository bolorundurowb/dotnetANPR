using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using dotnetANPR.Configuration;
using dotnetANPR.Utilities;
using Microsoft.Extensions.Logging;

namespace dotnetANPR.Intelligence.Parser;

public class Parser
{
    private static readonly ILogger<Parser> Logger = Logging.GetLogger<Parser>();

    private readonly List<PlateForm> _plateForms;

    public Parser()
    {
        _plateForms = [];
        var fileName = Configurator.Instance.GetPath("intelligence_syntaxDescriptionFile");

        if (string.IsNullOrEmpty(fileName))
            throw new IOException("Failed to get syntax description file from Configurator");

        try
        {
            _plateForms = LoadFromJsonc(fileName);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to load from parser syntax description file");
            throw;
        }
    }

    /// <summary>
    /// Loads plate format definitions from a JSONC file.
    /// The file must contain a top-level "plateFormats" array where each element
    /// has a "name" string and a "positions" array of allowed-character strings.
    /// </summary>
    public List<PlateForm> LoadFromJsonc(string fileName)
    {
        var plateForms = new List<PlateForm>();
        var json = File.ReadAllText(fileName);
        var docOptions = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip };

        using var doc = JsonDocument.Parse(json, docOptions);
        var root = doc.RootElement;

        if (!root.TryGetProperty("plateFormats", out var formatsArray))
            throw new IOException($"Syntax file '{fileName}' is missing the 'plateFormats' array.");

        foreach (var formatElement in formatsArray.EnumerateArray())
        {
            var name = formatElement.GetProperty("name").GetString()
                ?? throw new IOException("A plate format entry is missing its 'name' field.");

            var form = new PlateForm(name);

            if (!formatElement.TryGetProperty("positions", out var positionsArray))
                throw new IOException($"Plate format '{name}' is missing its 'positions' array.");

            foreach (var positionElement in positionsArray.EnumerateArray())
            {
                var content = positionElement.GetString()
                    ?? throw new IOException($"A position entry in format '{name}' is null.");

                // Characters are normalised to upper-case, matching the original XML loader.
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

    /// <summary>
    /// For the given length, finds a <see cref="PlateForm"/> of the same length.
    /// If no such <see cref="PlateForm"/> is found, tries to find one with fewer characters.
    /// </summary>
    /// <param name="length">The number of characters of the PlateForm.</param>
    /// <returns>A <see cref="PlateForm"/> of the specified length or shorter if available; otherwise, null.</returns>
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

    public void InvertFlags()
    {
        foreach (var form in _plateForms)
            form.IsFlagged = !form.IsFlagged;
    }

    /// <summary>
    /// Syntactically parses text from the given <see cref="RecognizedPlate"/> in the specified analysis mode.
    /// </summary>
    /// <param name="recognizedPlate">The plate to parse.</param>
    /// <param name="syntaxAnalysisMode">The mode in which to parse.</param>
    /// <returns>The parsed recognized plate text.</returns>
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
                // moving the form on the plate
                Logger.LogDebug("Comparing {} with form {} and offset {}.", recognizedPlate, form.Name, i);
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
                            finalPlate.RequiredChanges += rp.Cost / 100.0; // +x for its cost
                            finalPlate.AddChar(rp.Char);
                            break;
                        }
                    }
                }

                Logger.LogDebug("Adding {} with required changes {}.", finalPlate.Plate, finalPlate.RequiredChanges);
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
            Logger.LogDebug("Plate {} : {} with required changes {}.", i, finalPlates[i].Plate,
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

        public double RequiredChanges { get; internal set; }

        public void AddChar(char chr) { Plate += chr; }
    }

    #endregion
}
