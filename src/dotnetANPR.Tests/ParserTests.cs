using dotnetANPR.Intelligence;
using dotnetANPR.Intelligence.Parser;
using dotnetANPR.Recognizer;
using Microsoft.Extensions.Logging.Abstractions;

namespace dotnetANPR.Tests;

[TestClass]
public class ParserTests
{
    [TestMethod]
    public void Parse_DoNotParse_ReturnsRawText()
    {
        var syntaxPath = Path.Combine(AppContext.BaseDirectory, "Resources", "syntax.xml");
        var parser = new Parser(syntaxPath, NullLogger.Instance);
        var plate = new RecognizedPlate();
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 1f));
        rc.Sort(false);
        plate.AddCharacter(rc);

        var result = parser.Parse(plate, SyntaxAnalysisMode.DoNotParse);
        Assert.AreEqual("A", result);
    }
}
