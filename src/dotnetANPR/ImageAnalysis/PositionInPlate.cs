namespace DotNetANPR.ImageAnalysis;

public class PositionInPlate(int leftX, int rightX)
{
    public int LeftX { get; private set; } = leftX;

    public int RightX { get; private set; } = rightX;
}
