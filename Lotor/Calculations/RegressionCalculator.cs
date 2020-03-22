using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Calculations
{
    public class RegressionCalculator
    {
        /// <summary>
        /// calucates domains size 
        /// </summary>
        /// <param name="levels">domain levels like : 1,2,3</param>
        /// <param name="documentCountPerLevel">total number of documents found in each level of domain</param>
        /// <returns>size of domian / double </returns>
        public static double getDomainSize(double[] levels, double[] documentCountPerLevel)
        {
            // can be optimized
            double r, yin, slope;
            LinearRegression(levels, documentCountPerLevel, 0, documentCountPerLevel.Length, out r, out yin, out slope);
            return slope;
        }

        /// <summary>
        /// calulates the linear regression for given parameters
        /// </summary>
        private static void LinearRegression(double[] xVals, double[] yVals,
                                             int inclusiveStart, int exclusiveEnd,
                                             out double rsquared, out double yintercept,
                                             out double slope)
        {
            Debug.Assert(xVals.Length == yVals.Length);
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
    }
}
