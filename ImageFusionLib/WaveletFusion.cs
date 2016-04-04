using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WaveletFusion.Helpers;
using ImageFusionLib.MathMethods;

using WaveletFusion.Helpers;

namespace WaveletFusionLib
{
    public static class WaveletFusion
    {
        private static BitmapSource targetImage;
        private static BitmapSource objectImage;
        private static BitmapSource resultImage;

        public static BitmapSource FusionImages(BitmapSource pTargetImage, BitmapSource pObjectImage) {
            targetImage = pTargetImage;
            objectImage = pObjectImage;
            bool swapped = false;

            if (targetImage.Width < objectImage.Width)
            {
                swapped = true;
                BitmapSource temp = targetImage;
                targetImage = objectImage;
                objectImage = temp;
            }

            ApplyCoregister(ref targetImage, ref objectImage);

            if (swapped)
            {
                BitmapSource temp = targetImage;
                targetImage = objectImage;
                objectImage = temp;
            }

            targetImage = ApplyWaveletTransform(targetImage, true);
            //resultImage = targetImage;
            objectImage = ApplyWaveletTransform(objectImage, true);
            //resultImage = objectImage;

            resultImage = ApplyCoefficientFusion(targetImage, objectImage);

            resultImage = ApplyWaveletTransform(resultImage, false);



            return resultImage;
        }

        /// <summary>
        /// Coregister the target and object image attributes of the class. 
        /// The transformations are applied directly to the objectImage attribute. 
        /// </summary>
        private static unsafe void ApplyCoregister(ref BitmapSource targetImage, ref BitmapSource objectImage)
        {
            double xFactor = targetImage.PixelWidth / objectImage.PixelWidth;
            double yFactor = targetImage.PixelHeight / objectImage.PixelHeight;

            bool flag = false;
            if( xFactor == 4 ) {
                xFactor /= 2;
                yFactor /= 2;
                flag = true;
            }

            ImagePtr image = ImagePtr.FromBitmap(objectImage);
            image = image.Scale(xFactor, yFactor, ScaleMode.HighQuality);

            if( flag ) {
                byte* data = (byte*)image.Data.ToPointer();
                ImagePtr result = new ImagePtr(2 * image.Height, 2 * image.Width, PixelFormats.Gray8);
                byte* resData = (byte*)result.Data.ToPointer();
                byte val = data[0];

                Utils.UnsafeFill(resData, val, result.DataSize);

                for (int i = 0, x = 128; i < image.Height; ++i, ++x)
                    for (int j = 0, y = 128; j < image.Width; ++j, ++y)
                        resData[x * result.Width + y] = data[i * image.Width + j];

                objectImage = result.ToImageSource();
                return ;
            }

            objectImage = image.ToImageSource();
        }

        /// <summary>
        /// Transform to the Wavelet domain the data of the given image.
        /// </summary>
        /// <param name="forward">true to apply the Wavelet transform, false to apply the Inverse Wavelet Transform.</param>
        public static unsafe BitmapSource ApplyWaveletTransform(BitmapSource bitmap, bool forward)
        {
            ImagePtr imgPtr = ImagePtr.FromBitmap(bitmap);
            byte* data = (byte*)imgPtr.Data.ToPointer();

            int width = imgPtr.Width;
            int height = imgPtr.Height;

            //for (int i = 0; i < height; ++i)
            //{
            //    for (int j = 0; j < width; ++j)
            //    {
            //        data[i * width + j] = ((byte)(255 - data[i * width + j]));
            //    }
            //}

            double[,] pixels = new double[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pixels[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j]);
                }
            }

            if (forward)
            {
                DiscreteWaveletTransform.WaveletTransform(pixels);
            }
            else
            {
                DiscreteWaveletTransform.InverseWaveletTransform(pixels);
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    data[i * width + j] = (byte)Utils.Normalize(-1, 1, 0, 255, pixels[i, j]);
                }
            }

            //IFilter f = new PaletteFilter("");
            return imgPtr.ToImageSource();
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

            double[,] bmp1 = new double[height, width];
            double[,] bmp2 = new double[height, width];
            double[,] result = new double[height, width];
            
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    bmp1[i, j] = data1[i * width + j];
                    bmp2[i, j] = data2[i * width + j];
                }
            }

            // Matrix decomposition
            // res1 res2
            // res3 res4
            double[,] res2 = GaussianMeanFusion.Fusion(bmp1, bmp2, height >> 1, width >> 1, 0, width >> 1, height >> 1, width);
            double[,] res3 = GaussianMeanFusion.Fusion(bmp1, bmp2, height >> 1, width >> 1, height >> 1, 0, height, width >> 1);
            double[,] res4 = GaussianMeanFusion.Fusion(bmp1, bmp2, height >> 1, width >> 1, height>>1, width >> 1, height, width);
            
            // res1 is calculated in-place here
            int rowStart = 0;
            int colStart = 0;
            int rowLimit = height >> 1;
            int colLimit = width >> 1;            
            for (int i = rowStart; i < rowLimit; ++i)
            {
                for (int j = colStart; j < colLimit; ++j)
                {
                    result[i,j] = ( bmp1[i, j] + bmp2[i,j] ) / 2;
                }
            }

            // res2
            rowStart = 0; rowLimit = height >> 1;
            colStart = width >> 1; colLimit = width;
            for (int i = rowStart, x = 0; i < rowLimit; ++i, ++x)
            {
                for (int j = colStart, y = 0; j < colLimit; ++j, ++y)
                {
                    result[i, j] = res2[x, y];
                }
            }

            // res3
            rowStart = height >> 1; rowLimit = height;
            colStart = 0; colLimit = width >> 1;
            for (int i = rowStart, x = 0; i < rowLimit; ++i, ++x)
            {
                for (int j = colStart, y = 0; j < colLimit; ++j, ++y)
                {
                    result[i, j] = res3[x, y];
                }
            }

            // res4
            rowStart = height >> 1; rowLimit = height;
            colStart = width >> 1; colLimit = width;
            for (int i = rowStart, x = 0; i < rowLimit; ++i, ++x)
            {
                for (int j = colStart, y = 0; j < colLimit; ++j, ++y)
                {
                    result[i, j] = res4[x, y];
                }
            }

            ImagePtr res = new ImagePtr(width, height, PixelFormats.Gray8);            
            byte* resData = (byte*)res.Data.ToPointer();
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    resData[i * width + j] = (byte)result[i, j];
                }
            }

            return res.ToImageSource();
        }
    }
}
