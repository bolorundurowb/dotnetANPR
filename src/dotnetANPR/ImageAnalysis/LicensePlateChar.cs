using DotNetANPR.Config;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis
{
    public class LicensePlateChar : Photo
    {
        private readonly AppSettings _config;
        public PixelMap.Piece SourcePiece { get; }
        public float AverageBrightness { get; private set; }
        public float AverageSaturation { get; private set; }
        public float AverageHue { get; private set; }
        public float AverageContrast { get; private set; }
        
        private float[] _featureVector;

        public LicensePlateChar(Photo photo, PixelMap.Piece piece, AppSettings config) 
            : base(photo.GetBitmap().Copy()) // Takes ownership of a copy
        {
            _config = config;
            SourcePiece = piece;
            CalculateFeatures();
        }

        private void CalculateFeatures()
        {
            float sumBrightness = 0, sumSaturation = 0, sumHue = 0, sumContrast = 0;
            float[,] brightness = GetBrightnessMatrix();

            for(int y=0; y<Height; y++)
            {
                for(int x=0; x<Width; x++)
                {
                    var p = _bitmap.GetPixel(x, y);
                    float b = p.GetBrightness();
                    sumBrightness += b;
                    sumSaturation += p.GetSaturation();
                    sumHue += p.GetHue();
                    // Contrast calculation from original Java
                    sumContrast += (b - 0.5f) * (b - 0.5f) * 4; 
                }
            }
            AverageBrightness = sumBrightness / (Width * Height);
            AverageSaturation = sumSaturation / (Width * Height);
            AverageHue = sumHue / (Width * Height);
            AverageContrast = sumContrast / (Width * Height);
        }

        public float[] GetFeatureVector()
        {
            if (_featureVector != null) return _featureVector;

            int normX = _config.Recognition.CharNormalizedDimensionsX;
            int normY = _config.Recognition.CharNormalizedDimensionsY;
            int resizeMethod = _config.Recognition.CharResizeMethod;

            // Create a copy to resize
            using var clone = Clone();
            clone.Resize(normX, normY, resizeMethod == 0 ? SKFilterQuality.Low : SKFilterQuality.High);
            
            float[,] brightness = clone.GetBrightnessMatrix();
            _featureVector = new float[normX * normY];
            
            for (int y = 0; y < normY; y++)
            {
                for (int x = 0; x < normX; x++)
                {
                    _featureVector[y * normX + x] = brightness[x, y];
                }
            }
            return _featureVector;
        }
    }
}