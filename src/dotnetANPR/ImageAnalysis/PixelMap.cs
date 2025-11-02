using System.Collections.Generic;
using SkiaSharp;
using System.Drawing;
using System.Linq; // Using Point struct

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Replaces PixelMap.java. Finds connected components (characters).
/// </summary>
public class PixelMap
{
    public class Piece : List<Point>
    {
        public int MinX { get; private set; } = int.MaxValue;
        public int MaxX { get; private set; } = int.MinValue;
        public int MinY { get; private set; } = int.MaxValue;
        public int MaxY { get; private set; } = int.MinValue;

        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;
        public int CenterX => MinX + Width / 2;

        public Photo CreatePhoto(LicensePlate plate)
        {
            var pieceBitmap = new SKBitmap(Width, Height, plate.Info.ColorType, SKAlphaType.Opaque);
            using var canvas = new SKCanvas(pieceBitmap);
            canvas.Clear(SKColors.White); // Background is white
            var plateBrightness = plate.GetBrightnessMatrix();

            foreach (var p in this)
            {
                var color = ImageUtils.ToGrayscaleColor(plateBrightness[p.X, p.Y]);
                pieceBitmap.SetPixel(p.X - MinX, p.Y - MinY, color);
            }
            return new Photo(pieceBitmap);
        }

        public new void Add(Point p)
        {
            if (p.X < MinX) MinX = p.X;
            if (p.X > MaxX) MaxX = p.X;
            if (p.Y < MinY) MinY = p.Y;
            if (p.Y > MaxY) MaxY = p.Y;
            base.Add(p);
        }
    }

    private readonly int[,] _map; // 0=not visited, 1=visited, 2=queued
    private readonly float[,] _brightness;
    public int Width { get; }
    public int Height { get; }
    public List<Piece> Pieces { get; }

    public PixelMap(Photo photo)
    {
        _brightness = photo.GetBrightnessMatrix();
        Width = photo.Width;
        Height = photo.Height;
        _map = new int[Width, Height];
        Pieces = [];
        FindPieces();
    }

    private bool IsValid(int x, int y)
    {
        return _brightness[x, y] < 0.5f && _map[x, y] == 0;
    }

    private void FindPieces()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (IsValid(x, y))
                {
                    var piece = new Piece();
                    var queue = new Queue<Point>();
                    queue.Enqueue(new Point(x, y));
                    _map[x, y] = 2; // Queued

                    while (queue.Any())
                    {
                        var p = queue.Dequeue();
                        if (_brightness[p.X, p.Y] < 0.5f && _map[p.X, p.Y] != 1)
                        {
                            piece.Add(p);
                            _map[p.X, p.Y] = 1; // Visited

                            for (var ny = p.Y - 1; ny <= p.Y + 1; ny++)
                            {
                                for (var nx = p.X - 1; nx <= p.X + 1; nx++)
                                {
                                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && IsValid(nx, ny))
                                    {
                                        _map[nx, ny] = 2; // Queued
                                        queue.Enqueue(new Point(nx, ny));
                                    }
                                }
                            }
                        }
                    }
                    if (piece.Any()) Pieces.Add(piece);
                }
            }
        }
    }
}