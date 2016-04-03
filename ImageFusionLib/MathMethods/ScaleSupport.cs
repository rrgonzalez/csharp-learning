using System;
using System.Threading.Tasks;

using WaveletFusion.Helpers;

namespace ImageFusionLib.MathMethods
{
    public enum ScaleMode
    {
        LowQuality,
        HighQuality,
    }

    public static class ScaleSupport
    {
        #region Methods

        public static ImagePtr Scale(this ImagePtr source, double scaleX, double scaleY, ScaleMode method)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (Math.Abs(scaleX - 1) < double.Epsilon &&
                Math.Abs(scaleY - 1) < double.Epsilon)
                return (ImagePtr) source.Clone();

            int width = Math.Max(1, (int) (source.Width*scaleX + 0.5));
            int height = Math.Max(1, (int) (source.Height*scaleY + 0.5));

            var image =  source.CreateNew(width, height);

            switch (method)
            {
                case ScaleMode.LowQuality:
                    ScaleNearestNeighbor(source, image, scaleX, scaleY);
                    break;
                case ScaleMode.HighQuality:
                    if (source.PixelAsShort)
                    {
                        ScaleBiCubicPixelShort(source, image, scaleX, scaleY);
                    }
                    else
                    {
                        ScaleBiCubic(source, image, scaleX, scaleY);
                    }
                    break;
            }

            return image;
        }

        #endregion

        #region Helper Methods

        private static unsafe void ScaleNearestNeighbor(ImagePtr source, ImagePtr image, double scaleX, double scaleY)
        {
            int pixelSize = source.Format.BitsPerPixel/8;

            var sPtr = (byte*) source.Data.ToPointer();
            var iPtr = (byte*) image.Data.ToPointer();

            Parallel.For(
                0, image.Height,
                y =>
                    {
                        unchecked
                        {
                            int yw = y*image.Stride;

                            var y2 = (int) (y/scaleY);
                            int yw2 = y2*source.Stride;

                            for (int x = 0; x < image.Width; x++)
                            {
                                var x2 = (int) (x/scaleX);

                                ExternalMethod.CopyMemory(
                                    (IntPtr) (iPtr + yw + x*pixelSize),
                                    (IntPtr) (sPtr + yw2 + x2*pixelSize),
                                    (uint) pixelSize);
                            }
                        }
                    });
        }

        private static double BiCubicKernel(double x)
        {
            if (x > 2.0f) return 0.0f;

            double xm1 = x - 1.0f;
            double xp1 = x + 1.0f;
            double xp2 = x + 2.0f;

            double a = (xp2 <= 0.0f) ? 0.0f : xp2*xp2*xp2;

            double b = (xp1 <= 0.0f) ? 0.0f : xp1*xp1*xp1;

            double c = (x <= 0.0f) ? 0.0f : x*x*x;

            double d = (xm1 <= 0.0f) ? 0.0f : xm1*xm1*xm1;

            return (0.16666666666666666667f*(a - (4.0f*b) + (6.0f*c) - (4.0f*d)));
        }

        private static unsafe void ScaleBiCubic(ImagePtr source, ImagePtr image, double scaleX, double scaleY)
        {
            int pixelSize = source.Format.BitsPerPixel/8;

            int yMax = source.Height - 1;
            int xMax = source.Width - 1;

            var src = (byte*) source.Data.ToPointer();
            var dst = (byte*) image.Data.ToPointer();

            Parallel.For(
                0, image.Height,
                y =>
                    {
                        double oy = y/scaleY + 0.5;
                        var oy1 = (int) oy;
                        double dy = oy - oy1;

                        for (int x = 0; x < image.Width; x++)
                        {
                            double ox = x/scaleX + 0.5;
                            var ox1 = (int) ox;
                            double dx = ox - ox1;


                            for (int i = 0; i < pixelSize; i++)
                            {
                                double g = 0;

                                for (int n = -1; n < 3; n++)
                                {
                                    double k1 = BiCubicKernel(dy - n);

                                    int oy2 = oy1 + n;
                                    if (oy2 < 0) oy2 = 0;
                                    if (oy2 > yMax) oy2 = yMax;

                                    for (int m = -1; m < 3; m++)
                                    {
                                        double k2 = Math.Abs(k1) > double.Epsilon
                                                        ? k1*BiCubicKernel(m - dx)
                                                        : 0;

                                        int ox2 = ox1 + m;
                                        if (ox2 < 0) ox2 = 0;
                                        if (ox2 > xMax) ox2 = xMax;

                                        g += k2*src[oy2*source.Stride + ox2*pixelSize + i];
                                    }
                                }

                                dst[y*image.Stride + x*pixelSize + i] = (byte) g;
                            }
                        }
                    });
        }

        private static unsafe void ScaleBiCubicPixelShort(ImagePtr source, ImagePtr image, double scaleX, double scaleY)
        {
            int yMax = source.Height - 1;
            int xMax = source.Width - 1;

            var src = (short*) source.Data.ToPointer();
            var dst = (short*) image.Data.ToPointer();

            Parallel.For(
                0, image.Height,
                y =>
                    {
                        double oy = y/scaleY + 0.5;
                        var oy1 = (int) oy;
                        double dy = oy - oy1;

                        for (int x = 0; x < image.Width; x++)
                        {
                            double ox = x/scaleX + 0.5;
                            var ox1 = (int) ox;
                            double dx = ox - ox1;

                            double g = 0;

                            for (int n = -1; n < 3; n++)
                            {
                                double k1 = BiCubicKernel(dy - n);

                                int oy2 = oy1 + n;
                                if (oy2 < 0) oy2 = 0;
                                if (oy2 > yMax) oy2 = yMax;

                                for (int m = -1; m < 3; m++)
                                {
                                    double k2 = Math.Abs(k1) > double.Epsilon
                                                    ? k1*BiCubicKernel(m - dx)
                                                    : 0;

                                    int ox2 = ox1 + m;
                                    if (ox2 < 0) ox2 = 0;
                                    if (ox2 > xMax) ox2 = xMax;

                                    g += k2*src[oy2*source.Width + ox2];
                                }
                            }

                            dst[y*image.Width + x] = (short) g;
                        }
                    });
        }

        #endregion
    }
}
