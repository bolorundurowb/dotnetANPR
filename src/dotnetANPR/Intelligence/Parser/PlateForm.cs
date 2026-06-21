using System.Collections.Generic;

namespace dotnetANPR.Intelligence.Parser;

/// <summary>
/// Represents a licence plate format template (e.g. "AA-123-BB") with typed character positions.
/// </summary>
/// <param name="name">The descriptive name of the plate format.</param>
public class PlateForm(string name)
{
    /// <summary>The ordered list of allowed character sets for each position.</summary>
    public List<Position> Positions { get; private set; } = [];

    /// <summary>The number of character positions in this format.</summary>
    public int Length => Positions.Count;

    /// <summary>Whether this form is a candidate for the current recognition attempt.</summary>
    public bool IsFlagged { get; set; }

    /// <summary>The descriptive name of this format.</summary>
    public string Name { get; set; } = name;
}