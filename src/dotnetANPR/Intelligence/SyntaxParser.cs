using System.Collections.Generic;
using DotNetANPR.Config;
using DotNetANPR.Recognizer;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DotNetANPR.Intelligence;

public class SyntaxParser
{
    private readonly PlateSyntax _syntax;

    public SyntaxParser(AppSettings settings)
    {
        var jsonString = File.ReadAllText(settings.Recognition.SyntaxDescriptionFile);
        _syntax = JsonSerializer.Deserialize<PlateSyntax>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public RecognizedPlate Parse(List<RecognizedChar> chars, string syntaxName)
    {
        var syntax = _syntax.Syntaxes.FirstOrDefault(s => s.Name == syntaxName);
        if (syntax == null)
        {
            syntax = _syntax.Syntaxes.First(); // Fallback to default
        }

        var sb = new StringBuilder();
        double totalConfidence = 0;

        for (var i = 0; i < chars.Count; i++)
        {
            var recognizedChar = chars[i];
            recognizedChar.Sort();

            // Find the best pattern that matches the syntax
            var bestPattern = recognizedChar.Patterns.FirstOrDefault(p => syntax.IsCharAllowed(p.Character, i));

            if (bestPattern == null)
            {
                // No pattern matched syntax, just take the best guess
                bestPattern = recognizedChar.Patterns.First();
            }

            sb.Append(bestPattern.Character);
            totalConfidence += (1.0 - bestPattern.Similarity); // Similarity is error, so 1-S is confidence
        }

        var text = sb.ToString();
        var avgConfidence = totalConfidence / chars.Count;

        // This is where you would return the full RecognizedPlate,
        // but AnprEngine needs the plate object.
        // This method is simplified to return just the text/confidence.
        return new RecognizedPlate(text, avgConfidence, null); // Engine will attach plate
    }
}