using System.IO;
using DotNetANPR.Configuration;
using DotNetANPR.Intelligence;
using DotNetANPR.Intelligence.Parser;
using DotNetANPR.Recognizer;
using DotNetANPR.Utilities;
using Xunit;

namespace DotNetANPR.Tests;

public class ParserTests
{
    public ParserTests()
    {
        AnprConfig.Reset();
    }

    [Fact]
    public void Constructor_LoadsSyntaxFromEmbeddedResource()
    {
        var parser = new Parser();

        var plateForms = parser.LoadFromXml(ResourceHelper.OpenStream("Resources/syntax.xml")!);

        Assert.NotEmpty(plateForms);
    }

    [Fact]
    public void Parse_DoNotParse_ReturnsRawPlateText()
    {
        var parser = new Parser();
        var plate = new RecognizedPlate();
        plate.AddCharacter(CreateRecognizedCharacter('A'));
        plate.AddCharacter(CreateRecognizedCharacter('B'));
        plate.AddCharacter(CreateRecognizedCharacter('1'));

        var result = parser.Parse(plate, SyntaxAnalysisMode.DoNotParse);

        Assert.Equal("AB1", result);
    }

    private static Recognizer.RecognizedCharacter CreateRecognizedCharacter(char chr)
    {
        var rc = new Recognizer.RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern(chr, 0.1f));
        rc.Sort(false);
        return rc;
    }
}
