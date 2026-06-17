namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents the horizontal position of a detected element within a license plate image.
/// </summary>
/// <param name="leftX">The left x-coordinate boundary.</param>
/// <param name="rightX">The right x-coordinate boundary.</param>
public class PositionInPlate(int leftX, int rightX)
{
    /// <summary>
    /// Gets the left x-coordinate boundary of the position.
    /// </summary>
    public int LeftX { get; private set; } = leftX;

    /// <summary>
    /// Gets the right x-coordinate boundary of the position.
    /// </summary>
    public int RightX { get; private set; } = rightX;
}
