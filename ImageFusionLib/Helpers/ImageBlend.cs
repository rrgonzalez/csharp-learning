using System;
using System.Threading.Tasks;

namespace WaveletFusion.Helpers
{
    public enum BlendOperation
    {
        SourceCopy = 1,
        ROP_MergePaint,
        ROP_NOTSourceErase,
        ROP_SourceAND,
        ROP_SourceErase,
        ROP_SourceInvert,
        ROP_SourcePaint,
        Blend_Darken,
        Blend_Multiply,
        Blend_ColorBurn,
        Blend_Lighten,
        Blend_Screen,
        Blend_ColorDodge,
        Blend_Overlay,
        Blend_SoftLight,
        Blend_HardLight,
        Blend_PinLight,
        Blend_Difference,
        Blend_Exclusion,
        Blend_Hue,
        Blend_Saturation,
        Blend_Color,
        Blend_Luminosity
    }

    public static class ImageBlend
    {
        private delegate byte PerChannelProcessDelegate(ref byte nSrc, ref byte nDst);
        public static ImagePtr Blend(ImagePtr source1, ImagePtr source2, BlendOperation operation)
        {
            var result = new ImagePtr(source1.Width, source1.Height, source1.Format);
            switch (operation)
            {
                case BlendOperation.Blend_Darken:
                    PerChannelProcess(ref source1, source2,result, BlendDarken);
                    break;

                case BlendOperation.Blend_Multiply:
                    PerChannelProcess(ref source1, source2, result, BlendMultiply);
                    break;

                case BlendOperation.Blend_Screen:
                    PerChannelProcess(ref source1, source2, result, BlendScreen);
                    break;

                case BlendOperation.Blend_Lighten:
                    PerChannelProcess(ref source1, source2, result, BlendLighten);
                    break;

                case BlendOperation.Blend_HardLight:
                    PerChannelProcess(ref source1, source2, result, BlendHardLight);
                    break;

                case BlendOperation.Blend_Difference:
                    PerChannelProcess(ref source1, source2, result, BlendDifference);
                    break;

                case BlendOperation.Blend_PinLight:
                    PerChannelProcess(ref source1, source2, result, BlendPinLight);
                    break;

                case BlendOperation.Blend_Overlay:
                    PerChannelProcess(ref source1, source2, result, BlendOverlay);
                    ;
                    break;

                case BlendOperation.Blend_Exclusion:
                    PerChannelProcess(ref source1, source2, result, BlendExclusion);
                    break;

                case BlendOperation.Blend_SoftLight:
                    PerChannelProcess(ref source1, source2, result, BlendSoftLight);
                    break;

                case BlendOperation.Blend_ColorBurn:
                    PerChannelProcess(ref source1, source2, result, BlendColorBurn);
                    break;

                case BlendOperation.Blend_ColorDodge:
                    PerChannelProcess(ref source1, source2, result, BlendColorDodge);
                    break;
                case BlendOperation.ROP_SourcePaint:
                    PerChannelProcess(ref source1, source2, result, SourcePaint);
                    break;
            }
            return result;
        }
        private unsafe static void PerChannelProcess(ref ImagePtr source1, ImagePtr source2, ImagePtr result, PerChannelProcessDelegate ChannelProcessFunction)
        {
            int lenght = source1.DataSize;
            var src1 = (byte*)source1.Data;
            var src2 = (byte*)source2.Data;
            var dst = (byte*) result.Data;

            Parallel.For(0, lenght, i =>
            {
                dst[i] = ChannelProcessFunction(ref src1[i], ref src2[i]);
            });

        }

        #region Blend Pixels Functions ...
        // Choose darkest color 
        private static byte BlendDarken(ref byte Src, ref byte Dst)
        {
            return ((Src < Dst) ? Src : Dst);
        }

        // Multiply
        private static byte BlendMultiply(ref byte Src, ref byte Dst)
        {
            return (byte)Math.Max(Math.Min((Src / 255.0f * Dst / 255.0f) * 255.0f, 255), 0);
        }

        // Screen
        private static byte BlendScreen(ref byte Src, ref byte Dst)
        {
            return (byte)Math.Max(Math.Min(255 - ((255 - Src) / 255.0f * (255 - Dst) / 255.0f) * 255.0f, 255), 0);
        }

        // Choose lightest color 
        private static byte BlendLighten(ref byte Src, ref byte Dst)
        {
            return ((Src > Dst) ? Src : Dst);
        }

        // hard light 
        private static byte BlendHardLight(ref byte Src, ref byte Dst)
        {
            return ((Src < 128) ? (byte)Math.Max(Math.Min((Src / 255.0f * Dst / 255.0f) * 255.0f * 2, 255), 0) : (byte)Math.Max(Math.Min(255 - ((255 - Src) / 255.0f * (255 - Dst) / 255.0f) * 255.0f * 2, 255), 0));
        }

        // difference 
        private static byte BlendDifference(ref byte Src, ref byte Dst)
        {
            return (byte)((Src > Dst) ? Src - Dst : Dst - Src);
        }

        // pin light 
        private static byte BlendPinLight(ref byte Src, ref byte Dst)
        {
            return (Src < 128) ? ((Dst > Src) ? Src : Dst) : ((Dst < Src) ? Src : Dst);
        }

        // overlay 
        private static byte BlendOverlay(ref byte Src, ref byte Dst)
        {
            return ((Dst < 128) ? (byte)Math.Max(Math.Min((Src / 255.0f * Dst / 255.0f) * 255.0f * 2, 255), 0) : (byte)Math.Max(Math.Min(255 - ((255 - Src) / 255.0f * (255 - Dst) / 255.0f) * 255.0f * 2, 255), 0));
        }

        // exclusion 
        private static byte BlendExclusion(ref byte Src, ref byte Dst)
        {
            return (byte)(Src + Dst - 2 * (Dst * Src) / 255f);
        }

        // Soft Light (XFader formula)  
        private static byte BlendSoftLight(ref byte Src, ref byte Dst)
        {
            return (byte)Math.Max(Math.Min((Dst * Src / 255f) + Dst * (255 - ((255 - Dst) * (255 - Src) / 255f) - (Dst * Src / 255f)) / 255f, 255), 0);
        }

        // Color Burn 
        private static byte BlendColorBurn(ref byte Src, ref byte Dst)
        {
            return (Src == 0) ? (byte)0 : (byte)Math.Max(Math.Min(255 - (((255 - Dst) * 255) / Src), 255), 0);
        }

        // Color Dodge 
        private static byte BlendColorDodge(ref byte Src, ref byte Dst)
        {
            return (Src == 255) ? (byte)255 : (byte)Math.Max(Math.Min((Dst * 255) / (255 - Src), 255), 0);
        }

        // Source OR Destination
        private static byte SourcePaint(ref byte Src, ref byte Dst)
        {
            return (byte)Math.Max(Math.Min(Src | Dst, 255), 0);
        }

       
        #endregion
    }
}
