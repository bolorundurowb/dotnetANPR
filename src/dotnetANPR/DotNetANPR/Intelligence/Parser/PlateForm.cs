using System.Collections.Generic;

namespace DotNetANPR.Intelligence.Parser;

public class PlateForm
{
    public List<Position> Positions { get; private set; }

    public bool IsFlagged { get; set; }

    public string Name { get; set; }

    public PlateForm(string name)
    {
        Name = name;
        Positions = new List<Position>();
    }
}