using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace WaveletFusion.Helpers
{
    public static class BitmapSourceBuilder
    {
        #region Methods

        public static BitmapSource ToImageSource(this ImagePtr image)
        {
            if (image == null) throw new ArgumentNullException("image");

            return BitmapSource.Create(
                image.Width, image.Height, 96, 96,
                image.Format, null, image.Data, image.DataSize, image.Stride);
        }

        public static BitmapSource ToImageSource(this ImagePtr image, IEnumerable<IFilter> filters)
        {
            if (image == null) throw new ArgumentNullException("image");
            ImagePtr img = filters.Aggregate(image, (current, filter) => filter.Apply(current));
            return ToImageSource(img);
        }

        public static BitmapSource ToImageSource(this ImagePtr image, params IFilter[] filters)
        {
            if (filters == null) throw new ArgumentNullException("filters");
            return ToImageSource(image, (IEnumerable<IFilter>) filters);
        }

        #endregion

    }
}
