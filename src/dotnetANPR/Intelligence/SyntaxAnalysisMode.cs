namespace dotnetANPR.Intelligence;

/// <summary>
/// Specifies how the parser matches recognised text against known licence plate format templates.
/// </summary>
public enum SyntaxAnalysisMode
{
    /// <summary>No syntax analysis; returns the raw recognised text.</summary>
    DoNotParse = 0,

    /// <summary>Only match plate formats with the same number of characters.</summary>
    OnlyEqualLength = 1,

    /// <summary>Match plate formats with equal or fewer characters.</summary>
    EqualOrShorterLength = 2,
}