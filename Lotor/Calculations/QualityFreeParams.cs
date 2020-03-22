using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Calculations
{
    /// <summary>
    /// weights of each variable in quality estimation function
    /// </summary>
    class QualityFreeParams
    {
        public static double stopCoverC = 0.9;
        public static double ratioStops = 0.8;
        public static double entropy = 0.7;
        public static double numVisTermsC = 0.7;
        public static double ratioVisTextC = 0.4;
        public static double numTitleTermsC = 0.2;
        public static double avgTermLenC = 0.5;
        public static double urlDepthC = 0.5;
        public static double urlLengthC = 0.3;
        public static double ratioAnchorTextC = 0.3;
        public static double ratioTableTextC = 0.5;
    }
}
