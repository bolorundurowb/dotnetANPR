using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.ImageAnalysis
{
    class Char : Photo
    {
        public bool normalized = false;
        public PositionInPlate positionInPlate = null;

        //private PixelMap pixelMap;
        private PixelMap.Piece bestPiece = null;

        public int fullWidth, fullHeight, pieceWidth, pieceHeight;

        public float statisticAverageBrightness;
        public float statisticMinimumBrightness;
        public float statisticMaximumBrightness;
        public float statisticContrast;
        public float statisticAverageHue;
        public float statisticAverageSaturation;

        public Bitmap thresholdedImage;

        public Char()
        {
            image = null;
            init();
        }
        public Char(Bitmap bi, Bitmap thresholdedImage, PositionInPlate positionInPlate)
        {
            new Photo(bi);
            this.thresholdedImage = thresholdedImage;
            this.positionInPlate = positionInPlate;
            init();
        }
        public Char(Bitmap bi)
        {
            new Char(bi, bi, null);
            init();
        }
        // nacita znak zo suboru a hned vykona aj thresholding
        // prahovanie(thresholding) sa vacsinou u znakov nerobi, pretoze znaky sa vysekavaju
        // zo znacky, ktora uz je sama o sebe prahovana, ale nacitavanie zo suboru tomuto
        // principu nezodpoveda, cize spravime prahovanie zvlast :
        public Char(String filepath)
        {
            new Photo(filepath);
            // this.thresholdedImage = this.image; povodny kod, zakomentovany dna 23.12.2006 2:33 AM

            // nasledovne 4 riadky pridane 23.12.2006 2:33 AM
            Bitmap origin = Photo.duplicateBitmap(this.image);
            this.adaptiveThresholding(); // s ucinnostou nad this.image
            this.thresholdedImage = this.image;
            this.image = origin;

            init();
        }

        public Char clone()
        {
            return new Char((Bitmap)image.Clone(),
                    (Bitmap)thresholdedImage.Clone(), positionInPlate);
        }

        private void init()
        {
            this.fullWidth = base.getWidth();
            this.fullHeight = base.getHeight();
        }

        public void Normalize()
        {

            if (normalized) return;

            Bitmap colorImage = (Bitmap)this.getBi().Clone();
            this.image = this.thresholdedImage;
            PixelMap pixelMap = getPixelMap();

            this.bestPiece = pixelMap.getBestPiece();

            colorImage = getBestPieceInFullColor(colorImage, this.bestPiece);

            // vypocet statistik
            this.computeStatisticBrightness(colorImage);
            this.computeStatisticContrast(colorImage);
            this.computeStatisticHue(colorImage);
            this.computeStatisticSaturation(colorImage);

            this.image = this.bestPiece.render();

            if (this.image == null) this.image = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);

            this.pieceWidth = base.getWidth();
            this.pieceHeight = base.getHeight();

            this.normalizeResizeOnly();
            normalized = true;
        }

        private Bitmap getBestPieceInFullColor(Bitmap bi, PixelMap.Piece piece)
        {
            if (piece.width <= 0 || piece.height <= 0) return bi;
            return bi.Clone(new Rectangle(
                    piece.mostLeftPoint,
                    piece.mostTopPoint,
                    piece.width,
                    piece.height), PixelFormat.Format8bppIndexed);
        }

        private void normalizeResizeOnly()
        { // vracia ten isty Char, nie novy
            Configurator.Configurator configg = new Configurator.Configurator();
            int x = configg.getIntProperty("char_normalizeddimensions_x");
            int y = configg.getIntProperty("char_normalizeddimensions_y");
            if (x == 0 || y == 0) return;// nebude resize
            //this.linearResize(x,y);

            if (configg.getIntProperty("char_resizeMethod") == 0)
            {
                this.linearResize(x, y); // radsej weighted average
            }
            else
            {
                this.averageResize(x, y);
            }

            this.normalizeBrightness(0.5f);
        }


        private void computeStatisticContrast(Bitmap bi)
        {
            float sum = 0;
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    sum += Math.Abs(this.statisticAverageBrightness - getBrightness(bi, x, y));
                }
            }
            this.statisticContrast = sum / (w * h);
        }
        private void computeStatisticBrightness(Bitmap bi)
        {
            float sum = 0;
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;

            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float value = getBrightness(bi, x, y);
                    sum += value;
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                }
            }
            this.statisticAverageBrightness = sum / (w * h);
            this.statisticMinimumBrightness = min;
            this.statisticMaximumBrightness = max;
        }
        private void computeStatisticHue(Bitmap bi)
        {
            float sum = 0;
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    sum += getHue(bi, x, y);
                }
            }
            this.statisticAverageHue = sum / (w * h);
        }
        private void computeStatisticSaturation(Bitmap bi)
        {
            float sum = 0;
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    sum += getSaturation(bi, x, y);
                }
            }
            this.statisticAverageSaturation = sum / (w * h);
        }

        public PixelMap getPixelMap()
        {
            return new PixelMap(this);
        }

        ////////

        public List<Double> extractEdgeFeatures()
        {
            int w = this.image.Width;
            int h = this.image.Height;
            double featureMatch;

            float[,] array = BitmapToArrayWithBounds(this.image, w, h);
            w += 2; // pridame okraje
            h += 2;

            float[,] features = CharacterRecognizer.features;
            //List<Double> output = new List<Double>(features.length*4);
            double[] output = new double[features.Length * 4];

            for (int f = 0; f < features.Length; f++)
            { // cez vsetky features
                for (int my = 0; my < h - 1; my++)
                {
                    for (int mx = 0; mx < w - 1; mx++)
                    { // dlazdice x 0,2,4,..8 vcitane
                        featureMatch = 0;
                        featureMatch += Math.Abs(array[mx, my] - features[f, 0]);
                        featureMatch += Math.Abs(array[mx + 1, my] - features[f, 1]);
                        featureMatch += Math.Abs(array[mx, my + 1] - features[f, 2]);
                        featureMatch += Math.Abs(array[mx + 1, my + 1] - features[f, 3]);

                        int bias = 0;
                        if (mx >= w / 2) bias += features.Length; // ak je v kvadrante napravo , posunieme bias o jednu triedu
                        if (my >= h / 2) bias += features.Length * 2; // ak je v dolnom kvadrante, posuvame bias o 2 triedy
                        output[bias + f] += featureMatch < 0.05 ? 1 : 0;
                    } // end my
                } // end mx
            } // end f
            List<Double> outputList = new List<Double>();
            foreach (Double value in output) outputList.Add(value);
            return outputList;
        }

        public List<Double> extractMapFeatures()
        {
            List<Double> vectorInput = new List<Double>();
            for (int y = 0; y < this.getHeight(); y++)
                for (int x = 0; x < this.getWidth(); x++)
                    vectorInput.Add(this.getBrightness(x, y));
            return vectorInput;
        }

        public List<Double> extractFeatures()
        {
            Configurator.Configurator config = new Configurator.Configurator();
            int featureExtractionMethod = config.getIntProperty("char_featuresExtractionMethod");
            if (featureExtractionMethod == 0)
                return this.extractMapFeatures();
            else
                return this.extractEdgeFeatures();
        }


    }
}
