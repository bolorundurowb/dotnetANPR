using System;

namespace DotNetANPR.ImageAnalysis;

public struct Point(int x, int y) : IEquatable<Point>
{
    public int X { get; private set; } = x;

    public int Y { get; private set; } = y;

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public static bool operator ==(Point? left, Point? right) => Equals(left, right);

    public static bool operator !=(Point? left, Point? right) => !Equals(left, right);

    public bool Equals(Point other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Point other && Equals(other);
}
