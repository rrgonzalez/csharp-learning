using System;
using System.Runtime.InteropServices;

namespace WaveletFusion.Helpers
{
    internal static class ExternalMethod
    {
        [DllImport("kernel32.dll")]
        internal static extern void CopyMemory(IntPtr dest, IntPtr source, uint length);
    }
}
