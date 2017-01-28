using System.Drawing;

namespace dotNETANPR.ImageAnalysis.LookUp
{
    public class LookupOp
    {
        public int[] LookupTable { get; set; }

        public Bitmap Filter(Bitmap source)
        {
            Bitmap output = new Bitmap(source.Width, source.Height, source.PixelFormat);

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    Color sourceColor = source.GetPixel(i, j);

                    Color newColor = Color.FromArgb(
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
