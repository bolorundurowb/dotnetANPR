using dotnetANPR.Intelligence;

namespace dotnetANPR.Pipeline;

internal interface ISyntaxCorrector
{
    string Correct(RecognizedPlate plate, SyntaxAnalysisMode mode);
}
