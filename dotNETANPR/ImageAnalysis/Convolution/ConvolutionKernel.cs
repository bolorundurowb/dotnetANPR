namespace dotNETANPR.ImageAnalysis.Convolution
{
    public class ConvolutionKernel
    {
        public int Factor { get; set; }
        public int Offset { get; set; }

        private int[,] _matrix =
        {
            {-2, -2, -2, -2, -2},
            {-1, -1, -1, -1, -1},
            {0, 0, 0, 0, 0},
            {1, 1, 1, 1, 1},
            {2, 2, 2, 2, 2}
        };

        public int[,] Matrix
        {
            get
            {
                return _matrix;
            }
            set
            {
                _matrix = value;
                Factor = 0;

                for (var i = 0; i < Size; i++)
                {
                    for (var j = 0; j < Size; j++)
                    {
                        Factor += _matrix[i, j];
                    }
                }

                if (Factor == 0)
                {
                    Factor = 1;
                }
            }
        }

        private int _size = 5;

        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (value != 1 && value != 3 && value != 5 && value != 7)
                {
                    _size = 5;
                }
                else
                {
                    _size = value;
                }
            }
        }

        public ConvolutionKernel()
        {
            Offset = 0;
            Factor = 1;
        }
    }
}
