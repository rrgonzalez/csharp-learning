using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WaveletFusion.Helpers;
using ImageFusionLib.MathMethods;

namespace WaveletFusionLib
{
    public static class WaveletFusion
    {
        public static BitmapSource targetImage { get; set; }
        public static BitmapSource objectImage { get; set; }
        public static BitmapSource resultImage { get; set; }

        public static BitmapSource FusionImages(BitmapSource pTargetImage, BitmapSource pObjectImage) {
            targetImage = pTargetImage;
            objectImage = pObjectImage;

            //ApplyCoregister();

            targetImage = ApplyWaveletTransform(targetImage, true);
            resultImage = targetImage;
            //objectImage = ApplyWaveletTransform(objectImage, true);

            //resultImage = ApplyCoefficientFusion(targetImage, objectImage);

            //resultImage = ApplyWaveletTransform(resultImage, false);

            return resultImage;
        }

        /// <summary>
        /// Coregister the target and object image attributes of the class. 
        /// The transformations are applied directly to the objectImage attribute. 
        /// </summary>
        public static unsafe void ApplyCoregister() {
        }

        /// <summary>
        /// Transform to the Wavelet domain the data of the given image.
        /// </summary>
        /// <param name="forward">true to apply the Wavelet transform, false to apply the Inverse Wavelet Transform.</param>
        public static unsafe BitmapSource ApplyWaveletTransform(BitmapSource bitmap, bool forward)
        {
            var imgPtr = ImagePtr.FromBitmap(bitmap);
            byte* data = (byte*)imgPtr.Data.ToPointer();
            int width = imgPtr.Width;
            int height = imgPtr.Height;

            //double[,] red = new double[width, height];
            //double[,] green = new double[width, height];
            //double[,] blue = new double[width, height];
            double[,] extra = new double[width, height];

            byte pixel;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    pixel = data[j * width + i];
                    //red[i, j] = (double)pixel.R;
                    //green[i, j] = (double)pixel.G;
                    //blue[i, j] = (double)pixel.B;
                    //extra[i, j] = (double)pixel.S;
                    //red[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.R);
                    //green[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.G);
                    //blue[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.B);
                    extra[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel);
                }
            }

            if (forward)
            {
                //DiscreteWaveletTransform.WaveletTransform(red);
                //DiscreteWaveletTransform.WaveletTransform(green);
                //DiscreteWaveletTransform.WaveletTransform(blue);
                DiscreteWaveletTransform.WaveletTransform(extra);
            }
            else
            {
                //DiscreteWaveletTransform.InverseWaveletTransform(red);
                //DiscreteWaveletTransform.InverseWaveletTransform(green);
                //DiscreteWaveletTransform.InverseWaveletTransform(blue);
                DiscreteWaveletTransform.InverseWaveletTransform(extra);
            }

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    //data[i * width + j].R = (byte)red[i, j];
                    //data[i * width + j].G = (byte)green[i, j];
                    //data[i * width + j].B = (byte)blue[i, j];
                    //data[i * width + j].S = (byte)extra[i, j];
                    //data[i * width + j].R = (byte)Utils.Normalize(-1, 1, 0, 255, red[i, j]);
                    //data[i * width + j].G = (byte)Utils.Normalize(-1, 1, 0, 255, green[i, j]);
                    //data[i * width + j].B = (byte)Utils.Normalize(-1, 1, 0, 255, blue[i, j]);
                    data[j * width + i] = (byte)Utils.Normalize(-1, 1, 0, 255, extra[i, j]);
                }
            }

            return imgPtr.ToBitmapSource();
        }

        /// <summary>
        /// Applies the fusion rule to the coefficients of the images. 
        /// The result is saved in the attribute resultImage.
        /// </summary>
        public static unsafe BitmapSource ApplyCoefficientFusion(BitmapSource bitmap1, BitmapSource bitmap2)
        {
            var imgPtr1 = ImagePtr.FromBitmap(bitmap1);
            byte* data1 = (byte*)imgPtr1.Data.ToPointer();
            int width = imgPtr1.Width;
            int height = imgPtr1.Height;

            var imgPtr2 = ImagePtr.FromBitmap(bitmap2);
            byte* data2 = (byte*)imgPtr2.Data.ToPointer();

            double[,] bmp1 = new double[width, height];
            double[,] bmp2 = new double[width, height];
            double[,] result = new double[width, height];
            
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    bmp1[i, j] = *(data1 + (i * width + j));
                    bmp2[i, j] = *(data2 + (i * width + j));
                }
            }

            // upper right quarter
            result = GaussianMeanFusion.Fusion(bmp1, bmp2, height, width, 0, (width >> 1 - 1), (height >> 1 - 1), width);
            
            // down half
            result = GaussianMeanFusion.Fusion(bmp1, bmp2, height, width, (height >> 1), (width >> 1 - 1), (height >> 1 - 1), width);

            int rowLimit = height >> 1;
            int colLimit = width >> 1;
            for (int i = 0; i < rowLimit; ++i)
            {
                for (int j = 0; j < colLimit; ++j)
                {
                    result[i,j] = ( bmp1[i, j] + bmp2[i,j] ) / 2;
                }
            }

            ImagePtr res = new ImagePtr(width, height, false);
            byte* resData = (byte*)res.Data.ToPointer();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    *(resData + (i * width + j)) = (byte)result[i, j];
                }
            }

            return res.ToBitmapSource();
        }
    }
}
