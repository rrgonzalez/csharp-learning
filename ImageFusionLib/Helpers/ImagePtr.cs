using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WaveletFusion.Helpers
{
    public class ImagePtr : IDisposable, ICloneable
    {
        #region Fields

        private readonly int _dataSize;
        private readonly int _stride;
        private readonly int _width;
        private readonly int _height;
        private IntPtr _data;
        private readonly PixelFormat _format;
        private bool _disposed;
        private readonly bool _pixelAsShort;
        private BitmapPalette _palette;

        #endregion

        #region Constructors

        public ImagePtr(int width, int height, bool pixelAsShort)
            : this(width, height, PixelFormats.Gray16)
        {
            _pixelAsShort = pixelAsShort;
        }

        public ImagePtr(int width, int height, PixelFormat format)
            : this(IntPtr.Zero, width, height, format)
        {
            _data = MemoryManager.Alloc(_dataSize);

        }

        private ImagePtr(IntPtr ptr, int width, int height, bool pixelAsShort)
            : this(ptr, width, height, PixelFormats.Gray16)
        {
            _pixelAsShort = pixelAsShort;
        }

        private ImagePtr(IntPtr ptr, int width, int height, PixelFormat format)
        {
            _width = width;
            _height = height;
            _format = format;

            _stride = (_width * _format.BitsPerPixel + 7) / 8;
            _dataSize = _stride * _height;

            _data = ptr;

            GC.AddMemoryPressure(_dataSize);
        }

        #endregion

        #region Destructor

        ~ImagePtr()
        {
            DisposeHandler();
        }

        #endregion

        #region Properties

        public bool PixelAsShort
        {
            get { return _pixelAsShort; }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public IntPtr Data
        {
            get { return _data; }
        }

        public PixelFormat Format
        {
            get { return _format; }
        }

        public int Stride
        {
            get { return _stride; }
        }

        public int DataSize
        {
            get { return _dataSize; }
        }

        public BitmapPalette Palette
        {
            get { return _palette; }
        }

        #endregion

        #region Methods

        public static ImagePtr FromBitmap(BitmapSource source, Int32Rect rect)
        {
            if (source == null) throw new ArgumentNullException("source");

            int bpp = source.Format.BitsPerPixel;
            var stride = (bpp * source.PixelWidth + 7) / 8;

            var image = new ImagePtr(rect.Width, rect.Height, source.Format);
            source.CopyPixels(rect,image.Data,image.DataSize,stride);
            image._palette = source.Palette;
            return image;
        }

        public static ImagePtr FromBitmap(BitmapSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var rect = new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);
            return FromBitmap(source, rect);
        }

        public static ImagePtr FromFile(string fileName)
        {
            return FromBitmap(new BitmapImage(new Uri(fileName,UriKind.Relative)));
        }

        public static ImagePtr FromIntPtr(IntPtr ptr, int width, int height, PixelFormat format, bool cloneData)
        {
            int stride = (width * format.BitsPerPixel + 7) / 8;
            int dataSize = stride * height;

            IntPtr data = GetIntPtrData(ptr, dataSize, cloneData);

            return new ImagePtr(data, width, height, format);
        }

        public static ImagePtr FromIntPtr(IntPtr ptr, int width, int height, bool pixelAsShort, bool cloneData)
        {

            int stride = (width * 16 + 7) / 8;
            int dataSize = stride * height;

            IntPtr data = GetIntPtrData(ptr, dataSize, cloneData);

            return new ImagePtr(data, width, height, pixelAsShort);
        }

        public  BitmapSource ToBitmapSource()
        {
           return BitmapSource.Create(Width, Height, 96, 96,Format, null, Data, DataSize, Stride);
        }

        public static byte[] GetPixels(BitmapSource source)
        {
            var _stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            var arraBytes = new byte[_stride * source.PixelHeight];
            source.CopyPixels(arraBytes, _stride, 0);
            return arraBytes;
        }


        public object Clone()
        {
            ImagePtr img = CreateNew(Width, Height);

            CopyMemory(img.Data, Data, (uint)DataSize);

            return img;
        }

        public void Dispose()
        {
            DisposeHandler();
            GC.SuppressFinalize(this);
        }

        public ImagePtr CreateNew(int width, int height)
        {
            return PixelAsShort
                       ? new ImagePtr(width, height, true)
                       : new ImagePtr(width, height, Format);
        }

        public ImagePtr ClipRect(Int32Rect rect)
        {
            ImagePtr image = CreateNew(rect.Width, rect.Height);

            Copy(rect, image);

            return image;
        }

        #endregion

        #region Helper Methods

        private static IntPtr GetIntPtrData(IntPtr ptr, int size, bool cloneData)
        {
            if (!cloneData) return ptr;
            IntPtr imageData = Marshal.AllocHGlobal(size);
            CopyMemory(imageData, ptr, (uint)size);
            return imageData;
        }

        private void DisposeHandler()
        {
            if (_disposed) return;
            MemoryManager.Free(_data);
            GC.RemoveMemoryPressure(DataSize);
            _data = IntPtr.Zero;

            _disposed = true;
        }

        public unsafe void Copy(Int32Rect rect, ImagePtr ptr)
        {
            int bpp = Format.BitsPerPixel;

            var src = (byte*)Data.ToPointer();
            var dest = (byte*)ptr.Data.ToPointer();

            int x = (rect.X * bpp + 7) / 8;

            Parallel.For(
                0, rect.Height,
                i =>
                {
                    int y = rect.Y + i;

                    CopyMemory(
                        (IntPtr)(dest + i * ptr.Stride),
                        (IntPtr)(src + y * Stride + x),
                        (uint)ptr.Stride);
                });
        }

        [DllImport("kernel32.dll")]
        internal static extern void CopyMemory(IntPtr dest, IntPtr source, uint length);

        #endregion
    }

    public static class ImagePtrExtensions
    {
        public static ImagePtr Brightness(this ImagePtr source, double brightness)
        {
            var result = (ImagePtr) source.Clone();
            if (source.Format == PixelFormats.Gray8)
                 SetBrightnessGray8(result,(byte)brightness);
            if (source.Format == PixelFormats.Gray16)
                SetBrightnessGray16(result, (short)brightness);
            return result;
        }
        private unsafe static void SetBrightnessGray8(ImagePtr source, byte brightness)
        {
            int lenght = source.Width * source.Height;
            var src = (byte*)source.Data;
            Parallel.For(0, lenght, x =>
            {
                src[x] += brightness;
            });
        }
        private unsafe static void SetBrightnessGray16(ImagePtr source, short brightness)
        {
            int lenght = source.Width * source.Height;
            var src = (short*)source.Data;
            Parallel.For(0, lenght, x =>
            {
                src[x] += brightness;
            });
        }

        #region ContrastStretching

        public unsafe static void ContrastStretching(this ImagePtr source)
        {

            if (source.Format == PixelFormats.Gray8)
            {
                float min = 255;
                float max = 0;
                var src = (byte*)source.Data;

                Parallel.For(0, source.DataSize, i =>
                {
                    byte pixel = src[i];
                    if (min > pixel)
                        min = pixel;

                    if (max < pixel)
                        max = pixel;
                });

                Parallel.For(0, source.DataSize, i =>
                {
                    src[i] = (byte)((src[i] - min) * (255 / (max - min)));
                });
            }
            else if (source.Format == PixelFormats.Bgr32)
            {
                float minR = 255;
                float maxR = 0;

                float minG = 255;
                float maxG = 0;

                float minB = 255;
                float maxB = 0;

                float minS = 255;
                float maxS = 0;
                var src = (Bgr32*)source.Data;

                Parallel.For(0, source.Width * source.Height, i =>
                {
                    Bgr32 pixel = src[i];
                    if (minR > pixel.R)
                        minR = pixel.R;
                    if (maxR < pixel.R)
                        maxR = pixel.R;

                    if (minG > pixel.G)
                        minG = pixel.G;
                    if (maxG < pixel.G)
                        maxG = pixel.G;

                    if (minB > pixel.B)
                        minB = pixel.B;
                    if (maxB < pixel.B)
                        maxB = pixel.B;

                    if (minS > pixel.S)
                        minS = pixel.S;
                    if (maxS < pixel.S)
                        maxS = pixel.S;
                });

                Parallel.For(0, source.Width * source.Height, i =>
                {
                    Bgr32 pixel = src[i];
                    var r = (byte)((pixel.R - minR) * (255 / (maxR - minR)));
                    var g = (byte)((pixel.G - minG) * (255 / (maxG - minG)));
                    var b = (byte)((pixel.B - minB) * (255 / (maxB - minB)));
                    var s = (byte)((pixel.S - minS) * (255 / (maxS - minS)));
                    pixel.Set(r, g, b, s);
                });
            }
        }
        #endregion
    }
}
