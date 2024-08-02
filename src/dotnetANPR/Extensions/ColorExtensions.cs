using System;
using System.Drawing;

namespace DotNetANPR.Extensions;

internal static class ColorExtensions
{
    public static Color HsbToRgb(float hue, float saturation, float brightness)
    {
        // Clamp values to valid ranges
        hue = Math.Max(0.0f, Math.Min(1.0f, hue));
        saturation = Math.Max(0.0f, Math.Min(1.0f, saturation));
        brightness = Math.Max(0.0f, Math.Min(1.0f, brightness));

        int r = 0, g = 0, b = 0;

        if (saturation == 0)
        {
            r = g = b = (int)(brightness * 255.0f + 0.5f);
        }
        else
        {
            var h = (hue - (float)Math.Floor(hue)) * 6.0f;
            var f = h - (float)Math.Floor(h);
            var p = brightness * (1.0f - saturation);
            var q = brightness * (1.0f - saturation * f);
            var t = brightness * (1.0f - saturation * (1.0f - f));

            switch ((int)h)
            {
                case 0:
                    r = (int)(brightness * 255.0f + 0.5f);
                    g = (int)(t * 255.0f + 0.5f);
                    b = (int)(p * 255.0f + 0.5f);
                    break;
                case 1:
                    r = (int)(q * 255.0f + 0.5f);
                    g = (int)(brightness * 255.0f + 0.5f);
                    b = (int)(p * 255.0f + 0.5f);
                    break;
                case 2:
                    r = (int)(p * 255.0f + 0.5f);
                    g = (int)(brightness * 255.0f + 0.5f);
                    b = (int)(t * 255.0f + 0.5f);
                    break;
                case 3:
                    r = (int)(p * 255.0f + 0.5f);
                    g = (int)(q * 255.0f + 0.5f);
                    b = (int)(brightness * 255.0f + 0.5f);
                    break;
                case 4:
                    r = (int)(t * 255.0f + 0.5f);
                    g = (int)(p * 255.0f + 0.5f);
                    b = (int)(brightness * 255.0f + 0.5f);
                    break;
                case 5:
                    r = (int)(brightness * 255.0f + 0.5f);
                    g = (int)(p * 255.0f + 0.5f);
                    b = (int)(q * 255.0f + 0.5f);
                    break;
            }
        }

        return Color.FromArgb(r, g, b);
    }
}
