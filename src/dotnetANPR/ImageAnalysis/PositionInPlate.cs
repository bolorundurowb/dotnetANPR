namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Defines the horizontal position of a character within a licence plate, specified by its left and right pixel boundaries.
/// </summary>
/// <param name="leftX">The X-coordinate of the left edge.</param>
/// <param name="rightX">The X-coordinate of the right edge.</param>
public class PositionInPlate(int leftX, int rightX)
{
    /// <summary>Gets the X-coordinate of the left edge.</summary>
    public int LeftX { get; private set; } = leftX;

    /// <summary>Gets the X-coordinate of the right edge.</summary>
    public int RightX { get; private set; } = rightX;
}
