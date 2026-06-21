using dotnetANPR.Intelligence;
using dotnetANPR.Intelligence.Parser;

namespace dotnetANPR.Pipeline;

internal sealed class SyntaxCorrector : ISyntaxCorrector
{
    private readonly Parser _parser;

    public SyntaxCorrector(Parser parser) => _parser = parser;

    public string Correct(RecognizedPlate plate, SyntaxAnalysisMode mode) => _parser.Parse(plate, mode);
}
