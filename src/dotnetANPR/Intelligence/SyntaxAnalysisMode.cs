namespace DotNetANPR.Intelligence;

/// <summary>
/// Specifies the syntax analysis mode used when parsing a recognized license plate
/// against known plate format templates.
/// </summary>
public enum SyntaxAnalysisMode
{
    /// <summary>
    /// No syntax parsing is performed; the raw recognized text is returned as-is.
    /// </summary>
    DoNotParse = 0,

    /// <summary>
    /// Only plate format templates with the same length as the recognized plate are considered.
    /// </summary>
    OnlyEqualLength = 1,

    /// <summary>
    /// Plate format templates with equal or shorter length are considered,
    /// allowing for extra characters to be ignored.
    /// </summary>
    EqualOrShorterLength = 2,
}
