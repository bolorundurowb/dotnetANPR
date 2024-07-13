using System.Collections.Generic;

namespace DotNetANPR.Intelligence.Parser;

public class PlateForm(string name)
{
    public List<Position> Positions { get; private set; } = new();

    public bool IsFlagged { get; set; }

    public string Name { get; set; } = name;
}