using System.Collections.Generic;

namespace DotNetANPR.Intelligence.Parser;

/// <summary>
/// Defines a plate syntax template describing the expected format of a license plate.
/// Each template has a name and a sequence of <see cref="Position"/> entries that
/// specify which characters are allowed at each position.
/// </summary>
/// <param name="name">The name identifying this plate format (e.g., "EU standard").</param>
public class PlateForm(string name)
{
    /// <summary>
    /// Gets the list of character positions in this plate format template.
    /// </summary>
    public List<Position> Positions { get; } = new();

    /// <summary>
    /// Gets the number of character positions in this template.
    /// </summary>
    public int Length => Positions.Count;

    /// <summary>
    /// Gets or sets a value indicating whether this plate form has been flagged
    /// as a candidate during syntax analysis.
    /// </summary>
    public bool IsFlagged { get; set; }

    /// <summary>
    /// Gets or sets the name of this plate format template.
    /// </summary>
    public string Name { get; set; } = name;
}
