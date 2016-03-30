using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFusionLib.MathMethods
{
    class DiscreteWaveletTransform
    {
        private const double s0 = 0.5;
        private const double s1 = 0.5;
        private const double w0 = 0.5;
        private const double w1 = -0.5;
        private const int iterations = 1;

        /// <summary>
        /// Applies the 1D Haar Discrete Wavelet Transform.
        /// </summary>
        /// <param name="data">Vector of values to apply the transform.</param>
        private static void WaveletTransform(double[] data)
        {
            double[] temp = new double[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[i] = data[k] * s0 + data[k + 1] * s1; // w
                temp[i + h] = data[k] * w0 + data[k + 1] * w1; // d 
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
        }

        /// <summary>
        /// Applies the 2D Haar Discrete Wavelet Transform.
        /// </summary>
        /// <param name="data">Matrix of values to apply the transform.</param>
        public static void WaveletTransform(double[,] data)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            double[] row = new double[cols];
            double[] col = new double[rows];

            for (int it = 0; it < iterations; it++)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < row.Length; j++)
                        row[j] = data[i, j];

                    WaveletTransform(row);

                    for (int j = 0; j < row.Length; j++)
                        data[i, j] = row[j];
                }

                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < col.Length; i++)
                        col[i] = data[i, j];

                    WaveletTransform(col);

                    for (int i = 0; i < col.Length; i++)
                        data[i, j] = col[i];
                }
            }
        }

        /// <summary>
        /// Applies the 1D Haar Inverse Discrete Wavelet Transform.
        /// </summary>
        /// <param name="data">Vector of values to apply the inverse transform.</param>
        private static void InverseWaveletTransform(double[] data)
        {
            double[] temp = new double[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[k] = (data[i] * s0 + data[i + h] * w0) / w0;
                temp[k + 1] = (data[i] * s1 + data[i + h] * w1) / s0;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
        }

        /// <summary>
        /// Applies the 2D Haar Inverse Discrete Wavelet Transform.
        /// </summary>
        /// <param name="data">Matrix of values to apply the inverse transform.</param>
        public static void InverseWaveletTransform(double[,] data)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            double[] col = new double[rows];
            double[] row = new double[cols];

            for (int l = 0; l < iterations; l++)
            {
                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < row.Length; i++)
                        col[i] = data[i, j];

                    InverseWaveletTransform(col);

                    for (int i = 0; i < col.Length; i++)
                        data[i, j] = col[i];
                }

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < row.Length; j++)
                        row[j] = data[i, j];

                    InverseWaveletTransform(row);

                    for (int j = 0; j < row.Length; j++)
                        data[i, j] = row[j];
                }
            }
        }
    }
}
