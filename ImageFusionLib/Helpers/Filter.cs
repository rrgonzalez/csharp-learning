using System;
using System.Windows.Media;
//using Calib.Imaging;

using WaveletFusion.Exceptions;

namespace WaveletFusion.Helpers
{
    public abstract class Filter : IFilter
    {
        public const string InvalidImageSpectralResolutionMessage = "The pixel format of the images must be Gray8, Gray16, Bgr32, Bgr24 or Rgb24";

        protected struct RGB24
        {
            public byte R;
            public byte G;
            public byte B;
        }

        protected struct RGB48
        {
            public ushort R;
            public ushort G;
            public ushort B;
        }

        #region IFilter Member

        public virtual ImagePtr Apply(ImagePtr pixelData)
        {
            PixelAsShort = pixelData.PixelAsShort;

            PixelFormat pixelFormat = pixelData.Format;

            //Gray16
            if(pixelFormat == PixelFormats.Gray16)
            {
                return ApplyGray16(pixelData);
            }

            //Gray8
            if(pixelFormat == PixelFormats.Gray8)
            {
                return ApplyGray8(pixelData);
            }

            //Rgb24
            if(pixelFormat == PixelFormats.Rgb24)
            {
                return ApplyRgb24(pixelData);
            }

            //Rgb48
            if(pixelFormat == PixelFormats.Rgb48)
            {
                return ApplyRgb48(pixelData);
            }

            throw new InvalidImageResolutionException(InvalidImageSpectralResolutionMessage);
        }

        #endregion endregion

        #region Methods 

        protected abstract ImagePtr ApplyGray16(ImagePtr pixelData);

        protected abstract ImagePtr ApplyGray8(ImagePtr pixelData);

        protected abstract ImagePtr ApplyRgb48(ImagePtr pixelData);

        protected abstract ImagePtr ApplyRgb24(ImagePtr pixelData);

        #endregion

        #region Properties 

        protected bool PixelAsShort { get; private set; }

        #endregion
    }
}
