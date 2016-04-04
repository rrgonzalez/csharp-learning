using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFusionLib.MathMethods
{
    /// <summary>
    /// Gaussian weighted mean implementation for 2d matrix and size 5 kernel.
    /// </summary>
    /// The kernel is the given by default by Matlab.
    /// 0.0019    0.0201    0.0439    0.0201    0.0019
    /// 0.0201    0.2096    0.4578    0.2096    0.0201
    /// 0.0439    0.4578    1.0000    0.4578    0.0439
    /// 0.0201    0.2096    0.4578    0.2096    0.0201
    /// 0.0019    0.0201    0.0439    0.0201    0.0019
    class GaussianMeanFusion
    {
        // Only the first quarter, since it is a symmetrical matrix.
        private static double[,] kernel = { { 0.0019, 0.0201, 0.0439 },
                                            { 0.0201, 0.2096, 0.4578 }, 
                                            { 0.0439, 0.4578, 1.0000 } };

        /// <summary>
        /// Given the amount of neighboors, returns the sum of the weights from the Kernel. 
        /// </summary>
        /// <param name="neighboors"></param>
        /// <returns></returns>
        static private double WeightsSum(int neighboors) 
        {
            switch(neighboors) {
                case 9:
                    return 1.0 + 2 * 0.4578 + 2 * 0.0439 + 0.2096 + 2 * 0.0201 + 0.0019;
                case 12:
                    return 1.0 + 3 * 0.4578 + 2 * 0.0439 + 2 * 0.2096 + 3 * 0.0201 + 0.0019;
                case 15:
                    return 1.0 + 3 * 0.4578 + 3 * 0.0439 + 2 * 0.2096 + 4 * 0.0201 + 2 * 0.0019;
                case 16:
                    return 1.0 + 4 * 0.4578 + 2 * 0.0439 + 4 * 0.2096 + 4 * 0.0201 + 0.0019;
                case 20:
                    return 1.0 + 4 * 0.4578 + 3 * 0.0439 + 4 * 0.2096 + 6 * 0.0201 + 2 * 0.0019;
                default:
                    return 1.0 + 4 * 0.4578 + 4 * 0.0439 + 4 * 0.2096 + 8 * 0.0201 + 4 * 0.0019;
            };                
        }

        /// <summary>
        /// Given the difference in X and in Y, returns the corresponding weight.
        /// </summary>
        /// <param name="dx">Difference in X</param>
        /// <param name="dy">Difference in Y</param>
        /// <returns>double</returns>
        private static double Weight(int dx, int dy)
        {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            int x = 2, y = 2;

            return kernel[x - dx, y - dy];
        }

        /// <summary>
        /// Fuses 2 matrices into one, using the Gaussian Weighted Mean method.
        /// </summary>
        /// <param name="data1">First matrix of data</param>
        /// <param name="data2">Second matrix of data</param>
        /// <param name="startRow">Starting row to fuse (inclusive)</param>
        /// <param name="startCol">Starting col to fuse (inclusive)</param>
        /// <param name="endRow">End row to fuse (exclusive)</param>
        /// <param name="endCol">End col to fuse (exclusive)</param>
        /// <returns>double[,]</returns>
        public static double[,] Fusion(double[,] data1, double[,] data2, int height, int width,
                                        int startRow, int startCol, int endRow, int endCol)
        {
            double[,] result = new double[height, width];
            int neighboors, x, y;
            double sum, w;

            for (int i = startRow; i < endRow; ++i)
            {
                for (int j = startCol; j < endCol; ++j)
                {
                    sum = 0;
                    neighboors = 0;
                    for (int dx = -2; dx <= 2; ++dx)
                    {
                        for (int dy = -2; dy <= 2; ++dy)
                        {
                            x = i + dx; 
                            y = j + dy;
                            if( x >= startRow && x < endRow &&
                                y >= startCol && y < endCol)
                            {
                                ++neighboors;
                                w = Weight(dx, dy);
                                sum += ( data1[x, y] + data2[x, y] ) * w;
                            }
                        }
                    }

                    result[i - startRow, j - startCol] = sum / (2 * WeightsSum(neighboors));
                }
            }

            return result;
        }
    }
}
