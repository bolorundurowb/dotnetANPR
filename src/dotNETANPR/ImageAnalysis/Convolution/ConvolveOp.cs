using System;
using System.Drawing;

namespace dotNETANPR.ImageAnalysis.Convolution
{
    public class ConvolveOp
    {
        public Bitmap Convolve(Bitmap input, ConvolutionKernel kernel)
        {
            var output = new Bitmap(input.Width, input.Height);
            var s = kernel.Size / 2;

            for (var x = s; x < input.Width - s; x++)
            {
                for (var y = s; y < input.Height - s; y++)
                {
                    int r = 0, b = 0, g = 0;

                    for (var i = 0; i < kernel.Size; i++)
                    {
                        for (var j = 0; j < kernel.Size; j++)
                        {
                            var temp = input.GetPixel(x + i - s, y + j - s);

                            r += kernel.Matrix[i, j] * temp.R;
                            g += kernel.Matrix[i, j] * temp.G;
                            b += kernel.Matrix[i, j] * temp.B;
                        }
                    }

                    r = Math.Min(Math.Max(r / kernel.Factor + kernel.Offset, 0), 255);
                    g = Math.Min(Math.Max(g / kernel.Factor + kernel.Offset, 0), 255);
                    b = Math.Min(Math.Max(b / kernel.Factor + kernel.Offset, 0), 255);

                    output.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return output;
        }
    }
}
