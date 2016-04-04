using System;
using System.Windows.Media;
//using Calib.Imaging;

namespace WaveletFusion.Helpers
{
    public interface IFilter
    {
        ImagePtr Apply(ImagePtr pixelData);
    }


}
