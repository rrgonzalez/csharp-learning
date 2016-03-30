using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WaveletFusion.Helpers
{
    public static class BitmapSourceExtensions
    {
        
        #region Invert
        public unsafe static BitmapSource InvertColor(this BitmapSource source)
        {
           
            var result = ImagePtr.FromBitmap(source);
            var src = (byte*)result.Data;

            Parallel.For(0, result.DataSize, x =>
            {
                src[x] ^= 255;
            });
            var final = result.ToBitmapSource();
            result.Dispose();
            return final;
        }
        
        #endregion
        
        #region Gamma

        public static BitmapSource GammaCorrection(this ImageSource source, double factor)
        {

            var input = ImagePtr.FromBitmap((BitmapSource)source);
            var result = new ImagePtr(input.Width, input.Height, input.Format);
            if (result.Format == PixelFormats.Bgr32 || result.Format == PixelFormats.Indexed8)
                SetGammaGray8(input,result, factor);

            return result.ToBitmapSource();
        }

        private unsafe static void SetGammaGray8(ImagePtr source,ImagePtr result, double factor)
        {
            byte[] table = new byte[256];
            double g = 1 / factor;
            
            Parallel.For(0, 256, x =>
            {
                table[x] = (byte)Math.Min(255, (int)(Math.Pow(x / 255.0, g) * 255 + 0.5));
            });

            var src = (byte*)source.Data;
            var dst = (byte*) result.Data;
            Parallel.For(0, source.DataSize, x =>
            {
                dst[x] = table[src[x]];

            });
        }
      
        #endregion

        #region Sharpen
       
        public static BitmapSource Sharpen(this ImageSource source, double strength)
        {
            var input = ImagePtr.FromBitmap((BitmapSource)source);
            var result = new ImagePtr(input.Width, input.Height, input.Format);
            if (result.Format == PixelFormats.Bgr32 )
                SetSharpenBGR2(input, result, strength);
            var final = result.ToBitmapSource();
            final.Freeze();
            result.Dispose();
            return final;
        }
        
        private unsafe static void SetSharpenBGR(ImagePtr input,ImagePtr result, double strength)
        {
            var src = (Bgr32*)input.Data;
            var dst = (Bgr32*)result.Data;
            int width = input.Width;
            int height = input.Height;

            const int filterSize = 3;
            const int midFilterSize = filterSize / 2;

            var filter = new double[,]
            {
                {-1,-1,-1},
                {-1, 9,-1},
                {-1,-1,-1}
            };

            //double bias = 1- strength;
            double factor = strength;

            var yMax = height - midFilterSize;
            var xMax = width - midFilterSize;
            for (int y = midFilterSize; y < yMax; y++)
            {
                
                var y1 = y;
                Parallel.For(midFilterSize, xMax, x =>
                {
                    double red = 0.0, green = 0.0, blue = 0.0, Su = 0.0;

                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = (x - midFilterSize + filterX + width)%width;
                            int imageY = (y1 - midFilterSize + filterY + height)%height;

                            Bgr32 valueBgr32 = src[imageX*width + imageY];
                            var filterValue = filter[filterX, filterY];

                            red += valueBgr32.R*filterValue;
                            green += valueBgr32.G*filterValue;
                            blue += valueBgr32.B*filterValue;
                            Su += valueBgr32.S*filterValue;
                        }
                    }

                    var index = x * width + y1;
                    Bgr32 pixel = src[index];
                    var r = red * factor;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;

                    var g = green * factor;
                    if (g > 255) g = 255;
                    else if (g < 0) g = 0;

                    var b = blue * factor;
                    if (b > 255) b = 255;
                    else if (b < 0) b = 0;

                    var s = Su * factor;
                    if (s > 255) s = 255;
                    else if (s < 0) s = 0;

                    dst[index].Set((byte)r, (byte)g, (byte)b, (byte)s);

                });
            }
        }

        private unsafe static void SetSharpenBGR2(ImagePtr input, ImagePtr result, double strength)
        {
            var src = (Bgr32*)input.Data;
            var dst = (Bgr32*)result.Data;
            int width = input.Width;
            int height = input.Height;

            const int filterSize = 5;
            const int midFilterSize = filterSize / 2;
            var filter = new double[,]
                {
                    {-1, -1, -1, -1, -1},
                    {-1,  2,  2,  2, -1},
                    {-1,  2, 16,  2, -1},
                    {-1,  2,  2,  2, -1},
                    {-1, -1, -1, -1, -1}
                };

            double bias = 1.0 - strength;
            double factor = strength / 16;
        
            var yMax = height - midFilterSize;
            var xMax = width - midFilterSize;
            for (int y = midFilterSize; y < yMax; y++)
            {

                var y1 = y;
                Parallel.For(midFilterSize, xMax, x =>
                {
                    double red = 0.0, green = 0.0, blue = 0.0, Su = 0.0;

                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = (x - midFilterSize + filterX + width) % width;
                            int imageY = (y1 - midFilterSize + filterY + height) % height;

                            Bgr32 valueBgr32 = src[imageX * width + imageY];
                            var filterValue = filter[filterX, filterY];

                            red += valueBgr32.R * filterValue;
                            green += valueBgr32.G * filterValue;
                            blue += valueBgr32.B * filterValue;
                            Su += valueBgr32.S * filterValue;
                        }
                    }

                    var index = x * width + y1;
                    Bgr32 pixel = src[index];
                    var r = red * factor + pixel.R * bias;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;

                    var g = green * factor + pixel.G * bias;
                    if (g > 255) g = 255;
                    else if (g < 0) g = 0;

                    var b = blue * factor + pixel.B * bias;
                    if (b > 255) b = 255;
                    else if (b < 0) b = 0;

                    var s = Su * factor + pixel.S * bias;
                    if (s > 255) s = 255;
                    else if (s < 0) s = 0;

                    dst[index].Set((byte)r, (byte)g, (byte)b, (byte)s);

                });
            }
        }
        #endregion

        #region Emphasize
        public static BitmapSource Emphasize(this ImageSource source)
        {
            var input = ImagePtr.FromBitmap((BitmapSource)source);
            ImagePtr result = null;
            if (input.Format == PixelFormats.Bgr32)
               result= EmphasizeBGR(input);
            if(input.Format == PixelFormats.Gray8)
                result = EmphasizeGray(input);
            var final = result.ToBitmapSource();
            final.Freeze();
            result.Dispose();
            return final;
        }

        private static unsafe ImagePtr EmphasizeBGR(ImagePtr input)
        {
            var src = (Bgr32*)input.Data;
            
            int width = input.Width;
            int height = input.Height;
            var filteredImage = new ImagePtr(width, height, input.Format);
            var dst = (Bgr32*)filteredImage.Data;

            const int filterSize = 3;
            const int midFilterSize = filterSize / 2;

            var filter = new double[,]
            {
                {-1,-1,-1},
                {-1, 9,-1},
                {-1,-1,-1}
            };

            const double bias = 128.0;
            const double factor = 0.2;
            var yMax = height - midFilterSize;
            var xMax = width - midFilterSize;
            for (int y = midFilterSize; y < yMax; y++)
            {

                var y1 = y;
                Parallel.For(midFilterSize, xMax, x =>
                {
                    double red = 0.0, green = 0.0, blue = 0.0, Su = 0.0;

                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = (x - midFilterSize + filterX + width) % width;
                            int imageY = (y1 - midFilterSize + filterY + height) % height;

                            Bgr32 valueBgr32 = src[imageX * width + imageY];
                            var filterValue = filter[filterX, filterY];

                            red += valueBgr32.R * filterValue;
                            green += valueBgr32.G * filterValue;
                            blue += valueBgr32.B * filterValue;
                            Su += valueBgr32.S * filterValue;
                        }
                    }

                    var index = x * width + y1;

                    var r = factor * red + bias;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;

                    var g = factor * green + bias;
                    if (g > 255) g = 255;
                    else if (g < 0) g = 0;

                    var b = factor * blue + bias;
                    if (b > 255) b = 255;
                    else if (b < 0) b = 0;

                    var s = factor * Su + bias;
                    if (s > 255) s = 255;
                    else if (s < 0) s = 0;

                    dst[index].Set((byte)r, (byte)g, (byte)b, (byte)s);

                });
            }
            return ImageBlend.Blend(input, filteredImage, BlendOperation.Blend_Overlay);
          
        }

        private static unsafe ImagePtr EmphasizeGray(ImagePtr input)
        {
            var src = (byte*)input.Data;

            int width = input.Width;
            int height = input.Height;
            var filteredImage = new ImagePtr(width, height, input.Format);
            var dst = (byte*)filteredImage.Data;

            const int filterSize = 3;
            const int midFilterSize = filterSize / 2;

            var filter = new double[,]
            {
                {-1,-1,-1},
                {-1, 9,-1},
                {-1,-1,-1}
            };

            const double bias = 128.0;
            const double factor = 0.2;
            var yMax = height - midFilterSize;
            var xMax = width - midFilterSize;
            for (int y = midFilterSize; y < yMax; y++)
            {

                var y1 = y;
                Parallel.For(midFilterSize, xMax, x =>
                {
                    double gray = 0.0;

                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = (x - midFilterSize + filterX + width) % width;
                            int imageY = (y1 - midFilterSize + filterY + height) % height;

                            byte valueGray = src[imageX * width + imageY];
                            var filterValue = filter[filterX, filterY];

                            gray += valueGray;
                          
                        }
                    }

                    var index = x * width + y1;

                    var r = factor * gray + bias;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;

                    dst[index] = (byte) r;

                });
            }
            return ImageBlend.Blend(input, filteredImage, BlendOperation.Blend_Overlay);

        }

        #endregion

        #region NoiseReduction

        public static unsafe ImagePtr NoiseReduction( ImagePtr input)
        {
            var src = (Bgr32*)input.Data;

            int width = input.Width;
            int height = input.Height;
            var filteredImage = new ImagePtr(width, height, input.Format);
            var dst = (Bgr32*)filteredImage.Data;

            const int filterSize = 3;
            const int midFilterSize = filterSize / 2;

            var filter = new double[,]
            {
                {1,1,1},
                {1, 1,1},
                {1,1,1}
            };

            const double bias = 5.0;
            const double factor = 10;
            var yMax = height - midFilterSize;
            var xMax = width - midFilterSize;
            for (int y = midFilterSize; y < yMax; y++)
            {

                var y1 = y;
                Parallel.For(midFilterSize, xMax, x =>
                {
                    double red = 0.0, green = 0.0, blue = 0.0, Su = 0.0;

                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = (x - midFilterSize + filterX + width) % width;
                            int imageY = (y1 - midFilterSize + filterY + height) % height;

                            Bgr32 valueBgr32 = src[imageX * width + imageY];
                            var filterValue = filter[filterX, filterY];

                            red += valueBgr32.R * filterValue;
                            green += valueBgr32.G * filterValue;
                            blue += valueBgr32.B * filterValue;
                            Su += valueBgr32.S * filterValue;
                        }
                    }

                    var index = x * width + y1;

                    var r = factor * red + bias;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;

                    var g = factor * green + bias;
                    if (g > 255) g = 255;
                    else if (g < 0) g = 0;

                    var b = factor * blue + bias;
                    if (b > 255) b = 255;
                    else if (b < 0) b = 0;

                    var s = factor * Su + bias;
                    if (s > 255) s = 255;
                    else if (s < 0) s = 0;

                    dst[index].Set((byte)r, (byte)g, (byte)b, (byte)s);

                });
            }
            return filteredImage;

        }
        #endregion

        #region Shutter
        public static unsafe BitmapSource CircleShutter(this ImageSource source, Rect rect)
        {
            var result = ImagePtr.FromBitmap((BitmapSource)source);
            var input = (ImagePtr)result.Clone();
            var src = (Bgr32*)input.Data;
            int height = input.Height;
            int width = input.Width;
            for (int i = 0; i < height; i++)
            {
                var i1 = i;
                Parallel.For(0, width, j =>
                {
                    if (!IsInEllipse(j, i1, rect))
                    {
                        src[i1 * input.Width + j].S = 0;
                        src[i1 * input.Width + j].B = 0;
                        src[i1 * input.Width + j].G = 0;
                        src[i1 * input.Width + j].R = 0;
                    }
                });
            }
           
            return input.ToBitmapSource();
        }

        private static bool IsInEllipse(int x, int y, Rect rect)
        {
            double x0 = (rect.Left + rect.Right) / 2;
            double y0 = (rect.Top + rect.Bottom) / 2;

             var a = rect.Width / 2;
             var b = rect.Height / 2;
            return (Math.Pow((x - x0), 2) / (a * a)) + Math.Pow((y - y0), 2) / (b * b) <= 1;
        }

        public static bool isInRectangle(double centerX, double centerY, double radius,
    double x, double y)
        {
            return x >= centerX - radius && x <= centerX + radius &&
                y >= centerY - radius && y <= centerY + radius;
        }

        public static unsafe BitmapSource RectangleShutter(this ImageSource source, Rect rect)
        {

            var result = ImagePtr.FromBitmap((BitmapSource)source);
            var input = (ImagePtr)result.Clone();
            var src = (Bgr32*)input.Data;
            int height = input.Height;
            int width = input.Width;
            for (int i = 0; i < height; i++)
            {
                var i1 = i;
                Parallel.For(0, width, j =>
                {
                    if (!IsInRectangle(j, i1, rect))
                    {
                        src[i1 * input.Width + j].S = 0;
                        src[i1 * input.Width + j].B = 0;
                        src[i1 * input.Width + j].G = 0;
                        src[i1 * input.Width + j].R = 0;
                    }
                });
            }
            return input.ToBitmapSource();
        }

        public static BitmapSource DrawRegion(this ImageSource source, Rect rect)
        {
            var input = ImagePtr.FromBitmap((BitmapSource)source);
            var result = new ImagePtr((int)rect.Width, (int)rect.Height, PixelFormats.Bgr32);
            input.Copy(new Int32Rect((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height), result);
            return result.ToBitmapSource();
        }

        private static bool IsInRectangle(int x, int y, Rect rect)
        {
            return rect.Contains(x, y);
        }
        #endregion

        #region WindowsLevel
        public static BitmapSource WindowsLevel(this ImageSource source, double window, double level)
        {
            var converted = new FormatConvertedBitmap();
            converted.BeginInit();
            converted.Source = (BitmapSource) source;
            converted.DestinationFormat = PixelFormats.Gray8;
            converted.EndInit();
            var result = ImagePtr.FromBitmap(converted);
            var input = (ImagePtr)result.Clone();
            if (result.Format == PixelFormats.Bgr32 || result.Format == PixelFormats.Indexed8 || result.Format == PixelFormats.Gray8)
                WindowsLevelGray8(input, window,level);
            if (result.Format == PixelFormats.Gray16)
                WindowsLevelGray16(input, window, level);
            return input.ToBitmapSource();
        }

        private unsafe static void WindowsLevelGray8(ImagePtr source, double window, double level)
        {
            var src = (byte*)source.Data;
            var left = (byte) (level - 0.5 - (window - 1)/2);
            var right = (byte)(level - 0.5 + (window - 1) / 2);

            Parallel.For(0, source.DataSize, x =>
            {
                byte value = src[x];
                if (value != 0 && value != 255)
                {

                    if (value <= left)
                        value = 0;
                    else if (value > right)
                        value = 255;
                    else value = (byte)(((value - (level - 0.5)) / (window - 1) + 0.5) * 255);
                    src[x] = value;
                }

            });
        }
        private unsafe static void WindowsLevelGray16(ImagePtr source, double window, double level)
        {
            int lenght = source.Width * source.Height;
            var src = (short*)source.Data;
            Parallel.For(0, lenght, x =>
            {
                //src[x] += (short)brightness;
            });
        } 
        #endregion

        #region HistogramEqualization

        public unsafe static void Equalization(this ImagePtr source)
        {
            if (source.Format == PixelFormats.Bgr32)
            {
                int[] histogramG = new int[256];

                int numberOfPixels = source.Width * source.Height;
                var ptr = (Bgr32*)source.Data;

                Parallel.For(0, numberOfPixels, i =>
                {
                    Bgr32 pixel = ptr[i];
                    int mean = pixel.R + pixel.G + pixel.B + pixel.S;
                    mean /= 4;
                    histogramG[mean]++;
                    //histogramG[index] = histogramG[index] + 1;
                });

                byte[] equalizedHistogramG = Equalize(histogramG, numberOfPixels);
                Parallel.For(0, numberOfPixels, i =>
                {
                    Bgr32 pixel = ptr[i];
                    int mean = pixel.R + pixel.G + pixel.B + pixel.S;
                    mean /= 4;

                    ptr[i].S = ptr[i].R = ptr[i].G = ptr[i].B = (byte)equalizedHistogramG[mean];


                });
                //int[] histogramR = new int[256];
                //int[] histogramG = new int[256];
                //int[] histogramB = new int[256];

                //int[] histogramS = new int[256];
                //int numberOfPixels = source.Width * source.Height;
                //var ptr = (Bgr32*)source.Data;
                //Parallel.For(0, numberOfPixels, i =>
                //{
                //    histogramB[ptr[i].B]++;
                //    histogramG[ptr[i].G]++;
                //    histogramR[ptr[i].R]++;
                //    histogramS[ptr[i].S]++;
                //});

                //byte[] equalizedHistogramR = Equalize(histogramR, numberOfPixels);
                //byte[] equalizedHistogramG = Equalize(histogramG, numberOfPixels);
                //byte[] equalizedHistogramB = Equalize(histogramB, numberOfPixels);
                //byte[] equalizedHistogramS = Equalize(histogramS, numberOfPixels);
                //Parallel.For(0, numberOfPixels, i =>
                //{
                //    ptr[i].B = equalizedHistogramB[ptr[i].B];
                //    ptr[i].G = equalizedHistogramG[ptr[i].G];
                //    ptr[i].R = equalizedHistogramR[ptr[i].R];
                //    ptr[i].S = equalizedHistogramS[ptr[i].S];

                //});
            }
            else if (source.Format == PixelFormats.Gray8)
            {
                int[] histogramG = new int[256];
               
                int numberOfPixels = source.Width * source.Height;
                var ptr = (byte*)source.Data;

                Parallel.For(0, numberOfPixels, i =>
                {
                    int index = ptr[i];
                    histogramG[index] = histogramG[index] + 1;
                });

              
                byte[] equalizedHistogramG = Equalize(histogramG, numberOfPixels);
                Parallel.For(0, numberOfPixels, i =>
                {
                   
                    ptr[i] = (byte) equalizedHistogramG[ptr[i]];
                   

                });
                
            }
           
        }
        public unsafe static void Equalization2(this ImagePtr source)
        {
            byte pixel; int t;
            float[] histograma;
            float[] ecu; // tendrá el histograma acumulado
            float t2;
            int mayor, menor, mayor2 = 0;
            var src = (byte*) source.Data;
            pixel = src[0];
            t = pixel;
            mayor = t;
            menor = mayor;
            for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                {
                    pixel = src[y * source.Width + x];
                    t = pixel;

                    if (t > mayor)
                        mayor = t;

                    if (t < menor)
                        menor = t;
                }
            if (menor >= 0)
            {
                histograma = new float[mayor + 1];
                ecu = new float[histograma.Length];
            }

            else
            {
                mayor2 = 0 - menor;
                histograma = new float[mayor2 + mayor + 1];
                ecu = new float[histograma.Length];
            }
            for (int i = 0; i < histograma.Length; i++)
            { histograma[i] = 0; ecu[i] = 0; }


            for (int i = 0; i < source.Width; i++)
                for (int j = 0; j < source.Height; j++)
                {
                    pixel = src[i * source.Width + j];
                    t = pixel;
                    histograma[t]++;
                }

            for (int i = 0; i < histograma.Length; i++)
                histograma[i] /= (source.Height * source.Width);

            for (int i = 0; i < ecu.Length; i++)
                for (int j = 0; j < i; j++)
                    ecu[i] = ecu[i] + histograma[j];

            if (menor >= 0)

                for (int i = 0; i < source.Width; i++)
                    for (int j = 0; j < source.Height; j++)
                    {
                        pixel = src[i * source.Width + j];
                        t = pixel;
                        t2 = ecu[t] * (mayor - menor) + menor;
                        src[i * source.Width + j] = (byte)t2;
                     
                    }

            else

                for (int i = 0; i < source.Width; i++)
                    for (int j = 0; j < source.Height; j++)
                    {
                        pixel = src[i * source.Width + j];
                        t = pixel;
                        t2 = ecu[t + mayor2] * (mayor - menor) + menor;
                        src[i * source.Width + j] = (byte)t2;

                    }
        }

        // Histogram 
        private static byte[] Equalize(int[] histogram, long numPixel)
        {
            byte[] equalizedHistogram = new byte[256];
            //equalizedHistogram[0] = histogram[0] * histogram.Length / numPixel;
            //long prev = histogram[0];
            //Parallel.For(1, histogram.Length, i =>
            //{
            //    prev += histogram[i];
            //    equalizedHistogram[i] = prev * histogram.Length / numPixel;
            //});
            float coef = 255.0f / numPixel;

            // calculate the first value
            float prev = histogram[0] * coef;
            equalizedHistogram[0] = (byte)prev;
            Parallel.For(1, 256, i =>
            {
                prev += histogram[i] * coef;
                equalizedHistogram[i] = (byte)prev;
            });
           
            return equalizedHistogram;


            //float[] hist = new float[256];

            //hist[0] = histogram[0] * histogram.Length / numPixel;
            //long prev = histogram[0];
            //string str = "";
            //str += (int)hist[0] + "\n";

            //for (int i = 1; i < hist.Length; i++)
            //{
            //    prev += histogram[i];
            //    hist[i] = prev * histogram.Length / numPixel;
            //    str += (int)hist[i] + "   _" + i + "\t";
            //}

            ////	MessageBox.Show( str );
            //return hist;
        }
        #endregion

    }

    public struct Bgr32
    {
        public byte S;
        public byte B;
        public byte G;
        public byte R;

        public void Set(byte r, byte g, byte b, byte s)
        {
            R = r;
            G = g;
            B = b;
            S = s;
        }

        public Bgr32(byte s, byte b, byte g, byte r) : this()
        {
            S = s;
            B = b;
            G = g;
            R = r;  
        }

        public static Bgr32 operator -(Bgr32 c1, Bgr32 c2)
        {
            int S = c1.S - c2.S;
            if (S > 255) S = 255;
            else if (S < 0) S = 0;

            int R = c1.R - c2.R;
            if (R > 255) R = 255;
            else if (R < 0) R = 0;

            int G = c1.G - c2.G;
            if (G > 255) G = 255;
            else if (G < 0) G = 0;

            int B = c1.B - c2.B;
            if (B > 255) B = 255;
            else if (B < 0) B = 0;

           
            return new Bgr32((byte)S, (byte)B, (byte)G, (byte)R);
        }

     
        public static Bgr32 operator +(Bgr32 c1, Bgr32 c2)
        {
            int S = (int) Math.Log(c1.S - c2.S);
            if (S > 255) S = 255;
            if (S < 0) S = 0;

            int R = (int) Math.Log(c1.R - c2.R);
            if (R > 255) R = 255;
            if (R < 0) R = 0;

            int G = (int) Math.Log(c1.G - c2.G);
            if (G > 255) G = 255;
            if (G < 0) G = 0;

            int B = (int) Math.Log(c1.B - c2.B);
            if (B > 255) B = 255;
            if (B < 0) B = 0;


            return new Bgr32((byte)S, (byte)B, (byte)G, (byte)R);
        }
       
    }

}
