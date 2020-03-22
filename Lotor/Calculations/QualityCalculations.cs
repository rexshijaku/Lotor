using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Lotor.Calculations
{
    /// <summary>
    /// provides static functions to calculate the quality of a given document
    /// </summary>
    public class QualityCalculations
    {
        private const int DEPTH_NORM = 10;
        private const int AVG_TERM_LEN_NORM = 10;
        private const int LENGTH_NORM = 1000;
        private const int TITLE_NORM = 10;
        private const int MAX_URL_LEN = 2048;
        private const int ENTROPY_NORM = 10;

        public static IEnumerable<string> parseWords(string text)
        {
            return Regex.Matches(text.ToLower(), @"[\w-[\d_]]+")
                        .Cast<Match>()
                        .Select(m => m.Value);
        }

        public static double calRatioAnchorText(HtmlDocument documentHtml, int totalTermsCount)
        {
            HtmlNodeCollection allLinks = documentHtml.DocumentNode.SelectNodes("//*/a");
            StringBuilder sb = new StringBuilder();
            if (allLinks != null)
                foreach (HtmlNode link in allLinks)
                    sb.Append(link.InnerText + " ");

            int anchN = parseWords(sb.ToString()).ToArray().Length;
            double f = (double)anchN / totalTermsCount;
            if (f > 1.0)
                f = -1; // there are more links than text
            else if (f > 0.5 && f <= 1.0)
                f = -f;
            return f;
        }

        public static double calRatioTableText(HtmlDocument documentHtml, int totalTermCount)
        {
            string xPath = "//*/td";
            HtmlNodeCollection tdList = documentHtml.DocumentNode.SelectNodes(xPath);
            StringBuilder sb = new StringBuilder();
            if (tdList != null)
                foreach (HtmlNode td in tdList)
                    sb.Append(td.InnerText + " ");

            int tdN = parseWords(sb.ToString()).ToArray().Length;
            double f = (double)tdN / totalTermCount;
            if (f > 1.0)
                f = 1; // there are more links than text
            return -f;
        }

        public static double calRatioStops(string[] terms, List<string> stopWords)
        {
            int stopWCount = 0;
            foreach (string term in terms)
                if (stopWords.BinarySearch(term) >= 0)
                    stopWCount++;

            double f = (double)stopWCount / (terms.Length - stopWCount);
            return f;
        }

        public static double calStopCover(string[] terms, List<string> stopWords)
        {
            int stopWCount = 0;
            foreach (string sWord in stopWords)
                if (terms.Contains(sWord))
                    stopWCount++;

            double f = (double)stopWCount / (stopWords.Count);
            return f;
        }

        public static double calUrlDepth(string documentUrl)
        {
            documentUrl = documentUrl.Replace("http://", "");
            int n = documentUrl.Split('/').Length - 1;
            double f = (double)n / DEPTH_NORM;
            return -f;
        }

        public static double calAvgTermLen(string[] terms)
        {
            int sh = 0;
            foreach (string term in terms)
                sh += term.Length;

            double r = (double)sh / terms.Length;
            r = r / AVG_TERM_LEN_NORM;
            return r;
        }

        public static double calUrlLength(string documentUrl)
        {
            documentUrl = documentUrl.Replace("http://", "");
            double f = (double)documentUrl.Length / MAX_URL_LEN;
            return -f;
        }

        public static double calRatioVisText(string documentHtml, string documentText)
        {
            double f = (double)documentText.Length / documentHtml.Length;
            return f;
        }

        public static double calEntropy(string[] terms)
        {
            Dictionary<string, int> uniqueWords =
                terms.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

            int pdWi = 0;
            foreach (var uWord in uniqueWords)
                pdWi += uWord.Value;

            double H = 0;
            foreach (var uWord in uniqueWords)
            {
                float pi = uWord.Value / (float)pdWi;
                H += pi * Math.Log(pi, 2);
            }
            H = -H;
            return (H / ENTROPY_NORM);
        }

        public static double calNumVisTerms(string[] terms)
        {
            int length = terms.Length;
            if (terms.Length > LENGTH_NORM) length = LENGTH_NORM;
            double f = (double)length / LENGTH_NORM;
            return f;
        }

        public static double calNumTitleTerms(string documentHtml)
        {
            Match m = Regex.Match(documentHtml, @"<title>\s*(.+?)\s*</title>");
            if (m.Success)
            {
                string title = m.Groups[1].Value;
                double f = (double)parseWords(title).ToArray().Length / TITLE_NORM;
                return f;
            }
            else
                return 0.0;
        }
    }
}
