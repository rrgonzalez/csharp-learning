using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WaveletFusion.Helpers
{
    class FusionPaletteFilter : PaletteFilter
    {
        public FusionPaletteFilter(string lutFile)
            : base(lutFile)
        {

        }

        unsafe public ImagePtr Apply(ImagePtr pixelData, ImagePtr maskData)
        {
            int width = pixelData.Width;
            int heigth = pixelData.Height;
            var result = new ImagePtr(width, heigth, PixelFormats.Rgb24);
            var src = (byte*)pixelData.Data;
            var dst = (RGB24*)result.Data;

            var mask = (byte*)maskData.Data;

            Parallel.For(0, heigth, i =>
            {
                int y = i * width;
                for (int j = 0; j < width; j++)
                {
                    int xy = y + j;
                    if (mask[xy] > 3)
                    {
                        int index = src[xy];

                        dst[xy].R = (byte)(_r[index]);
                        dst[xy].G = (byte)(_g[index]);
                        dst[xy].B = (byte)(_b[index]);
                    }
                    else
                    {
                        dst[xy].R = src[xy];
                        dst[xy].G = src[xy];
                        dst[xy].B = src[xy];
                    }
                }

            });

            pixelData.Dispose();
            return result;
        }
    }
}
