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
    }
}
