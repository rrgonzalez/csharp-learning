using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WaveletFusion.Helpers;
using ImageFusionLib.MathMethods;
using WaveletFusion.Exceptions;

namespace WaveletFusionLib
{
    public static class WaveletFusion
    {
        private static ImagePtr targetImage;
        private static ImagePtr objectImage;
        private static ImagePtr resultImage;

        // Exceptions messages
        public const string NullImagePtrMessage = "The ImagePtr references passed to FuseImages method cannot be null.";
        public const string NullBitmapSourceMessage = "The BitmapSource references passed to FuseImages method cannot be null.";
        public const string InvalidImageSizeMessage = "One image must be 512x512 and the other 256x256";
        public const string InvalidImageSpectralResolutionMessage = "The pixel format of the images must be Gray8, Gray16, Bgr32, Bgr24 or Rgb24";

        public static BitmapSource FuseImages(BitmapSource pTargetImage, BitmapSource pObjectImage, string paletteFilter = "LUT/Hotiron.lut")
        {
            if( pTargetImage == null || pObjectImage == null )
                throw new NullReferenceException(NullBitmapSourceMessage);

            ImagePtr imgPtr1 = ImagePtr.FromBitmap(pTargetImage);
            ImagePtr imgPtr2 = ImagePtr.FromBitmap(pObjectImage);

            return FuseImages(imgPtr1, imgPtr2, paletteFilter).ToImageSource();
        }

        public static ImagePtr FuseImages(ImagePtr pTargetImage, ImagePtr pObjectImage, string paletteFilter = "LUT/Hotiron.lut")
        {
            if (pTargetImage == null || pObjectImage == null)
                throw new NullReferenceException(NullImagePtrMessage);

            if (!checkSize(pTargetImage, pObjectImage))
                throw new InvalidImageResolutionException(InvalidImageSizeMessage);

            targetImage = pTargetImage;
            objectImage = pObjectImage;
            bool swapped = false;

            if (targetImage.Width < objectImage.Width)
            {
                swapped = true;
                ImagePtr temp = targetImage;
                targetImage = objectImage;
                objectImage = temp;
            }

            ApplyCoregister(ref targetImage, ref objectImage);
            ImagePtr mask = ImagePtr.FromIntPtr(objectImage.Data, objectImage.Width, 
                objectImage.Height, objectImage.Format, true); // used for pseudo-coloring

            if (swapped)
            {
                ImagePtr temp = targetImage;
                targetImage = objectImage;
                objectImage = temp;
            }

            PaletteFilter pf = new PaletteFilter(paletteFilter);
            objectImage = pf.Apply(objectImage);

            targetImage = ApplyWaveletTransform(targetImage, true);
            objectImage = ApplyWaveletTransform(objectImage, true);

            resultImage = ApplyCoefficientFusion(targetImage, objectImage);

            return resultImage;
        }

        /// <summary>
        /// Coregister the target and object image attributes of the class. 
        /// The transformations are applied directly to the objectImage attribute. 
        /// </summary>
        private static unsafe void ApplyCoregister(ref ImagePtr targetImage, ref ImagePtr objectImage)
        {
            double xFactor = targetImage.Width / objectImage.Width;
            double yFactor = targetImage.Height / objectImage.Height;

            bool flag = false;
            if( xFactor == 4 ) {
                xFactor /= 2;
                yFactor /= 2;
                flag = true;
            }

            ImagePtr image = objectImage.Scale(xFactor, yFactor, ScaleMode.HighQuality);

            if( flag ) {
                byte* data = (byte*)image.Data.ToPointer();
                ImagePtr result = new ImagePtr(2 * image.Height, 2 * image.Width, PixelFormats.Gray8);
                byte* resData = (byte*)result.Data.ToPointer();
                byte val = data[0];

                Utils.UnsafeFill(resData, val, result.DataSize);

                for (int i = 0, x = 128; i < image.Height; ++i, ++x)
                    for (int j = 0, y = 128; j < image.Width; ++j, ++y)
                        resData[x * result.Width + y] = data[i * image.Width + j];

                objectImage = result;
                return ;
            }

            objectImage = image;
        }

        /// <summary>
        /// Transform to the Wavelet domain the data of the given image.
        /// </summary>
        /// <param name="forward">true to apply the Wavelet transform, false to apply the Inverse Wavelet Transform.</param>
        private static unsafe ImagePtr ApplyWaveletTransform(ImagePtr bitmap, bool forward)
        {
            ImagePtr imgPtr = bitmap;

            if (bitmap.Format == PixelFormats.Bgr32)
            {
                Bgr32* data = (Bgr32*)imgPtr.Data;

                int width = imgPtr.Width;
                int height = imgPtr.Height;

                double[,] red = new double[height, width];
                double[,] green = new double[height, width];
                double[,] blue = new double[height, width];
                double[,] extra = new double[height, width];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        red[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].R);
                        green[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].G);
                        blue[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].B);
                        extra[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].S);
                    }
                }

                if (forward)
                {
                    DiscreteWaveletTransform.WaveletTransform(red);
                    DiscreteWaveletTransform.WaveletTransform(green);
                    DiscreteWaveletTransform.WaveletTransform(blue);
                    DiscreteWaveletTransform.WaveletTransform(extra);
                }
                else
                {
                    DiscreteWaveletTransform.InverseWaveletTransform(red);
                    DiscreteWaveletTransform.InverseWaveletTransform(green);
                    DiscreteWaveletTransform.InverseWaveletTransform(blue);
                    DiscreteWaveletTransform.InverseWaveletTransform(extra);
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        data[i * width + j].R = (byte)Utils.Normalize(-1, 1, 0, 255, red[i, j]);
                        data[i * width + j].G = (byte)Utils.Normalize(-1, 1, 0, 255, green[i, j]);
                        data[i * width + j].B = (byte)Utils.Normalize(-1, 1, 0, 255, blue[i, j]);
                        data[i * width + j].S = (byte)Utils.Normalize(-1, 1, 0, 255, extra[i, j]);
                    }
                }

                return imgPtr;
            }
            else if (bitmap.Format == PixelFormats.Bgr24 || bitmap.Format == PixelFormats.Rgb24)
            {
                Bgr24* data = (Bgr24*)imgPtr.Data;

                int width = imgPtr.Width;
                int height = imgPtr.Height;

                double[,] red = new double[height, width];
                double[,] green = new double[height, width];
                double[,] blue = new double[height, width];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        red[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].R);
                        green[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].G);
                        blue[i, j] = Utils.Normalize(0, 255, -1, 1, data[i * width + j].B);
                    }
                }

                if (forward)
                {
                    DiscreteWaveletTransform.WaveletTransform(red);
                    DiscreteWaveletTransform.WaveletTransform(green);
                    DiscreteWaveletTransform.WaveletTransform(blue);
                }
                else
                {
                    DiscreteWaveletTransform.InverseWaveletTransform(red);
                    DiscreteWaveletTransform.InverseWaveletTransform(green);
                    DiscreteWaveletTransform.InverseWaveletTransform(blue);
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        data[i * width + j].R = (byte)Utils.Normalize(-1, 1, 0, 255, red[i, j]);
                        data[i * width + j].G = (byte)Utils.Normalize(-1, 1, 0, 255, green[i, j]);
                        data[i * width + j].B = (byte)Utils.Normalize(-1, 1, 0, 255, blue[i, j]);
                    }
                }

                return imgPtr;
            }
            else if (bitmap.Format == PixelFormats.Gray16)
            {
                short* data = (short*)imgPtr.Data.ToPointer();

                int width = imgPtr.Width;
                int height = imgPtr.Height;

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
                        data[i * width + j] = (short)Utils.Normalize(-1, 1, 0, 255, pixels[i, j]);
                    }
                }

                return imgPtr;
            }
            else if (bitmap.Format == PixelFormats.Gray8)
            {
                byte* data = (byte*)imgPtr.Data.ToPointer();

                int width = imgPtr.Width;
                int height = imgPtr.Height;

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

                return imgPtr;
            }
            else
                throw new InvalidImageResolutionException(InvalidImageSpectralResolutionMessage);
        }

        /// <summary>
        /// Applies the fusion rule to the coefficients of the images. 
        /// The result is saved in the attribute resultImage.
        /// </summary>
        private static unsafe ImagePtr ApplyCoefficientFusion(ImagePtr targetImage, ImagePtr objectImage)
        {
            var imgPtrTarget = targetImage;
            byte* targetData = (byte*)imgPtrTarget.Data.ToPointer();
            int width = imgPtrTarget.Width;
            int height = imgPtrTarget.Height;            

            var imgPtrObject = objectImage;

            Bgr24* objectData = (Bgr24*)imgPtrObject.Data;
            //byte* objectData = (byte*)imgPtrObject.Data.ToPointer();

            double[,] targetBmp = new double[height, width];

            double[,] objectBmpRed = new double[height, width];
            double[,] objectBmpGreen = new double[height, width];
            double[,] objectBmpBlue = new double[height, width];

            double[,] resultBmpRed = new double[height, width];
            double[,] resultBmpGreen = new double[height, width];
            double[,] resultBmpBlue = new double[height, width];
            
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    targetBmp[i, j] = targetData[i * width + j];

                    objectBmpRed[i, j] = objectData[i * width + j].R;
                    objectBmpGreen[i, j] = objectData[i * width + j].G;
                    objectBmpBlue[i, j] = objectData[i * width + j].B;
                }
            }

            // Matrix decomposition
            // res1 res2
            // res3 res4
            double[,] res2Red = GaussianMeanFusion.Fusion(targetBmp, objectBmpRed, height >> 1, width >> 1, 0, width >> 1, height >> 1, width);
            double[,] res3Red = GaussianMeanFusion.Fusion(targetBmp, objectBmpRed, height >> 1, width >> 1, height >> 1, 0, height, width >> 1);
            double[,] res4Red = GaussianMeanFusion.Fusion(targetBmp, objectBmpRed, height >> 1, width >> 1, height>>1, width >> 1, height, width);

            double[,] res2Green = GaussianMeanFusion.Fusion(targetBmp, objectBmpGreen, height >> 1, width >> 1, 0, width >> 1, height >> 1, width);
            double[,] res3Green = GaussianMeanFusion.Fusion(targetBmp, objectBmpGreen, height >> 1, width >> 1, height >> 1, 0, height, width >> 1);
            double[,] res4Green = GaussianMeanFusion.Fusion(targetBmp, objectBmpGreen, height >> 1, width >> 1, height >> 1, width >> 1, height, width);

            double[,] res2Blue = GaussianMeanFusion.Fusion(targetBmp, objectBmpBlue, height >> 1, width >> 1, 0, width >> 1, height >> 1, width);
            double[,] res3Blue = GaussianMeanFusion.Fusion(targetBmp, objectBmpBlue, height >> 1, width >> 1, height >> 1, 0, height, width >> 1);
            double[,] res4Blue = GaussianMeanFusion.Fusion(targetBmp, objectBmpBlue, height >> 1, width >> 1, height >> 1, width >> 1, height, width);
            
            // res1 is calculated in-place here
            int rowStart = 0;
            int colStart = 0;
            int rowLimit = height >> 1;
            int colLimit = width >> 1;

            for (int i = rowStart; i < rowLimit; ++i)
            {
                for (int j = colStart; j < colLimit; ++j)
                {
                    resultBmpRed[i,j] = ( targetBmp[i, j] + objectBmpRed[i,j] ) / 2;
                    resultBmpGreen[i, j] = (targetBmp[i, j] + objectBmpGreen[i, j]) / 2;
                    resultBmpBlue[i, j] = (targetBmp[i, j] + objectBmpBlue[i, j]) / 2;
                }
            }

            // res2
            rowStart = 0; rowLimit = height >> 1;
            colStart = width >> 1; colLimit = width;
            for (int i = rowStart, x = 0; i < rowLimit; ++i, ++x)
            {
                for (int j = colStart, y = 0; j < colLimit; ++j, ++y)
                {
                    resultBmpRed[i, j] = res2Red[x, y];
                    resultBmpGreen[i, j] = res2Green[x, y];
                    resultBmpBlue[i, j] = res2Blue[x, y];
                }
            }

            // res3
            rowStart = height >> 1; rowLimit = height;
            colStart = 0; colLimit = width >> 1;
            for (int i = rowStart, x = 0; i < rowLimit; ++i, ++x)
            {
                for (int j = colStart, y = 0; j < colLimit; ++j, ++y)
                {
                    resultBmpRed[i, j] = res3Red[x, y];
                    resultBmpGreen[i, j] = res3Green[x, y];
                    resultBmpBlue[i, j] = res3Blue[x, y];
                }
            }

            // res4
            rowStart = height >> 1; rowLimit = height;
            colStart = width >> 1; colLimit = width;
            for (int i = rowStart, x = 0; i < rowLimit; ++i, ++x)
            {
                for (int j = colStart, y = 0; j < colLimit; ++j, ++y)
                {
                    resultBmpRed[i, j] = res4Red[x, y];
                    resultBmpGreen[i, j] = res4Green[x, y];
                    resultBmpBlue[i, j] = res4Blue[x, y];
                }
            }

            ImagePtr res = new ImagePtr(width, height, PixelFormats.Bgr24);
            Bgr24* resData = (Bgr24*)res.Data;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    resData[i * width + j].B = (byte)resultBmpRed[i, j];
                    resData[i * width + j].G = (byte)resultBmpGreen[i, j];
                    resData[i * width + j].R = (byte)resultBmpBlue[i, j];
                }
            }

            return res;
        }

        private static bool checkSize(ImagePtr pTargetImage, ImagePtr pObjectImage) {
            if (pTargetImage.Width != pTargetImage.Height || pObjectImage.Width != pObjectImage.Height)
                return false;

            if (pTargetImage.Width < pObjectImage.Width)
            {
                ImagePtr temp = pTargetImage;
                pTargetImage = pObjectImage;
                pObjectImage = temp;
            }

            if (pTargetImage.Width != 512 || ( pObjectImage.Width != 256 && pObjectImage.Width != 128) )
                return false;

            return true;
        }
    }
}
