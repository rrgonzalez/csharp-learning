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

            ApplyCoregister();

            targetImage = ApplyWaveletTransform(targetImage, true);
            objectImage = ApplyWaveletTransform(objectImage, true);

            //targetImage = ApplyWaveletTransform(targetImage, false);

            ApplyCoefficientFusion();

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
            Bgr32* data = (Bgr32*)imgPtr.Data;
            int width = imgPtr.Width;
            int height = imgPtr.Height;

            double[,] red = new double[width, height];
            double[,] green = new double[width, height];
            double[,] blue = new double[width, height];
            double[,] extra = new double[width, height];

            Bgr32 pixel;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    pixel = data[i * width + j];
                    //red[i, j] = (double)pixel.R;
                    //green[i, j] = (double)pixel.G;
                    //blue[i, j] = (double)pixel.B;
                    //extra[i, j] = (double)pixel.S;
                    red[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.R);
                    green[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.G);
                    blue[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.B);
                    extra[i, j] = (double)Utils.Normalize(0, 255, -1, 1, pixel.S);
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

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    //data[i * width + j].R = (byte)red[i, j];
                    //data[i * width + j].G = (byte)green[i, j];
                    //data[i * width + j].B = (byte)blue[i, j];
                    //data[i * width + j].S = (byte)extra[i, j];
                    data[i * width + j].R = (byte)Utils.Normalize(-1, 1, 0, 255, red[i, j]);
                    data[i * width + j].G = (byte)Utils.Normalize(-1, 1, 0, 255, green[i, j]);
                    data[i * width + j].B = (byte)Utils.Normalize(-1, 1, 0, 255, blue[i, j]);
                    data[i * width + j].S = (byte)Utils.Normalize(-1, 1, 0, 255, extra[i, j]);
                }
            }

            return imgPtr.ToBitmapSource();
        }

        /// <summary>
        /// Applies the fusion rule to the coefficients of the images. 
        /// The result is saved in the attribute resultImage.
        /// </summary>
        public static unsafe void ApplyCoefficientFusion()
        {
        }
    }
}
