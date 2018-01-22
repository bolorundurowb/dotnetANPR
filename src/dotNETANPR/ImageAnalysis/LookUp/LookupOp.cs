using System.Drawing;

namespace dotnetANPR.ImageAnalysis.LookUp
{
    public class LookupOp
    {
        public int[] LookupTable { get; set; }

        public Bitmap Filter(Bitmap source)
        {
            var output = new Bitmap(source.Width, source.Height, source.PixelFormat);

            for (var i = 0; i < source.Width; i++)
            {
                for (var j = 0; j < source.Height; j++)
                {
                    var sourceColor = source.GetPixel(i, j);

                    var newColor = Color.FromArgb(
                        sourceColor.A,
                        LookupTable[sourceColor.R],
                        LookupTable[sourceColor.G],
                        LookupTable[sourceColor.B]
                    );
                    output.SetPixel(i, j, newColor);
                }
            }
            return output;
        }
    }
}
