using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletFusion.Helpers
{
    class Utils
    {
        /// <summary>
        /// Translate values from some range [a,b] (origin range) to other range [x,y] (destiny range).
        /// </summary>
        /// <param name="fromMin">Lower bound of the origin range.</param>
        /// <param name="fromMax">Upper bound of the origin range.</param>
        /// <param name="toMin">Lower bound of the destiny range.</param>
        /// <param name="toMax">Upper bound of the destiny range.</param>
        /// <param name="value">Value to normalize or translate.</param>
        /// <returns></returns>
        public static double Normalize(double fromMin, double fromMax, double toMin, double toMax, double value)
        {
            if (fromMax - fromMin == 0) return 0;
            value = (toMax - toMin) * (value - fromMin) / (fromMax - fromMin) + toMin;

            if (value > toMax)
                return toMax;
            
            if (value < toMin)
                return toMin;

            return value;
        }

        /// <summary>
        /// Fast way to fill an array with a single value.
        /// </summary>
        /// <param name="arr">A pointer to the array to fill</param>
        /// <param name="value">Value to fill with</param>
        /// <param name="arrLength">Length of the array</param>
        /// <returns>byte[]</returns>
        public static unsafe void UnsafeFill(byte* arr, byte value, int arrLength)
        {
            Int64 fillValue = BitConverter.ToInt64(new[] { value, value, value, value, value, value, value, value }, 0);
            Int64* src = &fillValue;

            var dest = (Int64*)arr;
            while (arrLength >= 8)
            {
                *dest = *src;
                dest++;
                arrLength -= 8;
            }

            var bDest = (byte*)dest;
            for (byte i = 0; i < arrLength; i++)
            {
                *bDest = value;
                bDest++;
            }
            
        }
    }
}
