using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
//using Calib.Imaging;

namespace WaveletFusion.Helpers
{
    public class PaletteFilter:Filter
    {
        #region Fields 

        private readonly ushort[] _r;
        private readonly ushort[] _g;
        private readonly ushort[] _b;
        private bool _fromLut = false;

        #endregion

        #region Constructors 

        public PaletteFilter(string lutFile)
        {
            _r = _g = _b = new ushort[256];
            _fromLut = true;
            LoadFromLUTFile(lutFile);
        }

        public PaletteFilter(ushort[] r, ushort[] g, ushort[] b)
        {
            _r = r;
            _g = g;
            _b = b;
        }


        #endregion

        #region Methods


        unsafe protected override ImagePtr ApplyGray16(ImagePtr pixelData)
        {
            int width = pixelData.Width;
            int heigth = pixelData.Height;
            var result = new ImagePtr( width, heigth, PixelFormats.Rgb48);
            var src = (ushort*)pixelData.Data;
            var dst = (RGB48*)result.Data;
            
            Parallel.For(0, heigth, i =>
            {

                int y = i*width;
          
                for (int j = 0; j < width; j++)
                {
                    int xy = y + j;
                    int index = src[xy];
                    

                    dst[xy].R = _r[index];
                    dst[xy].G = _g[index];
                    dst[xy].B = _b[index];
                }
           
            });
            pixelData.Dispose();
            return result;
        }

        unsafe protected override ImagePtr ApplyGray8(ImagePtr pixelData)
        {
            int width = pixelData.Width;
            int heigth = pixelData.Height;
            var result = new ImagePtr(width, heigth, PixelFormats.Rgb24);
            var src = (byte*)pixelData.Data;
            var dst = (RGB24*)result.Data;

            Parallel.For(0, heigth, i =>
            {
                int y = i * width;
                for (int j = 0; j < width; j++)
                {
                    int xy = y + j;
                    int index = src[xy];

                    dst[xy].R = (byte)(_r[index] >> 8);
                    dst[xy].G = (byte)(_g[index]>>8);
                    dst[xy].B = (byte)(_b[index]>>8);
                }

            });
            pixelData.Dispose();
            return result;
        }

        unsafe protected override ImagePtr ApplyRgb48(ImagePtr pixelData)
        {
            int width = pixelData.Width;
            int heigth = pixelData.Height;
            var result = new ImagePtr(width, heigth, PixelFormats.Rgb48);
            var src = (RGB48*)pixelData.Data;
            var dst = (RGB48*)result.Data;

            Parallel.For(0, heigth, i =>
            {

                int y = i * width;

                for (int j = 0; j < width; j++)
                {
                    int xy = y + j;
                    int index = (src[xy].R + src[xy].G + src[xy].B) / 3;

                    dst[xy].R = _r[index];
                    dst[xy].G = _g[index];
                    dst[xy].B = _b[index];
                }

            });
            pixelData.Dispose();
            return result;
        }

        unsafe protected override ImagePtr ApplyRgb24(ImagePtr pixelData)
        {
            int width = pixelData.Width;
            int heigth = pixelData.Height;
            var result = new ImagePtr(width, heigth, PixelFormats.Rgb24);
            var src = (RGB24*)pixelData.Data;
            var dst = (RGB24*)result.Data;

            Parallel.For(0, heigth, i =>
            {

                int y = i * width;

                for (int j = 0; j < width; j++)
                {
                    int xy = y + j;
                    int index = (src[xy].R + src[xy].G + src[xy].B)/3;

                    dst[xy].R = (byte)(_r[index] >> 8);
                    dst[xy].G = (byte)(_g[index] >> 8);
                    dst[xy].B = (byte)(_b[index] >> 8);
                }

            });
            pixelData.Dispose();
            return result;
        }

        #endregion

        #region Helper Methods 
        
        private void LoadFromLUTFile(string fileName)
        {
            Stream sReader = File.Open(fileName, FileMode.Open);
            var reader = new BinaryReader(sReader);
            for (int i = 0; i < 256; i++)
                _r[i] = reader.ReadByte();
            for (int i = 0; i < 256; i++)
                _g[i] = reader.ReadByte();
            for (int i = 0; i < 256; i++)
                _b[i] = reader.ReadByte();
            sReader.Close();
            reader.Close();
        }

        #endregion
    }
}
