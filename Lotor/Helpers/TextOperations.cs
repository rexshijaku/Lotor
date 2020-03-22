using Lotor.Caches;
using Lotor.Globals;
using Lotor.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lotor.Helpers
{
    class TextOperations
    {
        #region Extracts
        /// <summary>
        /// extracts text from documents html
        /// </summary>
        /// <param name="documentHtml">documents html</param>
        /// <param name="url">the url of the document</param>
        /// <param name="filterWordsByLength">should words whos lengths is equal or lower be filtered out</param>
        /// <returns></returns>
        public static string getDocumentsText(string documentHtml, string url, bool filterWordsByLength = true)
        {
            Report.info(url + " getting document text...");
            string documentText = String.Empty;
            if (!String.IsNullOrEmpty(documentHtml))
            {
                HtmlDocument doc = new HtmlDocument();
                documentHtml = WebUtility.HtmlDecode(documentHtml);
                doc.LoadHtml(documentHtml);

                string xPath = "//*[not(self::script or self::style or self::a)]/text()[not(normalize-space(.)='')]";
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes(xPath);
                if (collection != null)
                    foreach (HtmlNode node in collection)
                        documentText += (node.InnerText.Trim() + " ");
            }
            documentText = documentText.ToLower();
            foreach (var char_ in MainCache.charsToReplace)
                documentText = documentText.Replace(char_.Key, char_.Value);

            Regex rgx = new Regex("[^a-zA-Z ]");
            documentText = rgx.Replace(documentText, "");
            if (filterWordsByLength)
            {
                Regex removeShortWords = new Regex(@"\b\w{1,max_word_length}\b".Replace("max_word_length", (Configs.MIN_WORD_LENGTH - 1).ToString()));
                documentText = removeShortWords.Replace(documentText, "");
            }
            documentText = Regex.Replace(documentText, @"\s+", " ");
            documentText = documentText.Trim();
            return documentText;
        }


        /// <summary>
        /// Grabs the text of all anchors found in a given html
        /// </summary>
        /// <param name="documentHtml">Html of document from which we want to extract anchors</param>
        /// <returns></returns>
        private static string getAnchorText(string documentHtml)
        {
            string content = "";
            if (!String.IsNullOrEmpty(documentHtml))
            {
                HtmlDocument doc = new HtmlDocument();
                documentHtml = WebUtility.HtmlDecode(documentHtml);
                doc.LoadHtml(documentHtml);

                string xPath;

                if (DomainCache.activeDomain.indexPage.hasUnorderedList())
                    xPath = "//ul//li/a";
                else
                    xPath = "//*/a";

                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes(xPath);
                if (collection != null)
                {
                    foreach (HtmlNode node in collection)
                    {
                        var hrefAttr = node.Attributes["href"];
                        if (hrefAttr != null)
                        {
                            string text = node.InnerText.Trim();
                            if (!String.IsNullOrEmpty(text))
                                content += (text + " ");
                        }
                    }
                }

                content = content.ToLower();
                foreach (var char_ in MainCache.charsToReplace)
                    content = content.Replace(char_.Key, char_.Value);

                Regex allowedCharsReg = new Regex("[^a-zA-Z ]");
                content = allowedCharsReg.Replace(content, "");

                Regex wordCountReg = new Regex(@"\b\w{1," + (Configs.MIN_WORD_LENGTH - 1) + "}\b"); // remove all words shorter than some length chars
                content = wordCountReg.Replace(content, "");
                content = Regex.Replace(content, @"\s+", " ");
            }
            return content;
        }

        /// <summary>
        /// extracts links from documents html
        /// </summary>
        /// <param name="document">document whos links should be extracted</param>
        /// <returns>extracted links as list</returns>
        public static List<string> extractLinks(Document document)
        {
            Report.info(document.url + " extracting links...");
            List<string> links = new List<string>();
            if (!String.IsNullOrEmpty(document.html))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(document.html);
                string xPath = "//*[self::a or self::area]";
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes(xPath);
                if (collection != null)
                {
                    foreach (HtmlNode link in collection)
                    {
                        var hrefAttr = link.Attributes["href"];
                        if (hrefAttr != null)
                            links.Add(hrefAttr.Value);
                    }
                }
            }
            return links;
        }

        public static int imageCount(string documentHtml)
        {
            if (!String.IsNullOrEmpty(documentHtml))
            {
                HtmlDocument doc = new HtmlDocument();
                documentHtml = WebUtility.HtmlDecode(documentHtml);
                doc.LoadHtml(documentHtml);

                string xpath = "//img";
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes(xpath);
                if (collection != null)
                    return collection.Count;
            }
            return 0;
        }
        #endregion

        #region Estimating
        /// <summary>
        /// finds Albanian value of a document
        /// </summary>
        /// <param name="documentText"></param>
        /// <returns></returns>
        public static double getAlbValue(string documentText)
        {
            int albWordCount = 0;
            string[] allWords = documentText.Split(' ');
            double wordCount = allWords.Count();

            foreach (var word in allWords)
                if (MainCache.MostCommonAlbanianWords.ContainsKey(word))
                    albWordCount++;

            if (wordCount > 0)
            {
                double albRatio = (double)albWordCount / wordCount;
                if (!Double.IsNaN(albRatio))
                    return albRatio;
            }
            return 0.0;
        }

        /// <summary>
        /// Calculates the language ratio of Anchors 
        /// </summary>
        /// <param name="documentHtml">documents html</param>
        /// <returns></returns>
        public static double getAnchorsAlbRatio(string documentHtml)
        {
            string documentText = getAnchorText(documentHtml).ToLower();

            if (documentText.Split(' ').Length <= 3)
                return -1.0;

            double wordCount = 0;
            int albWordCount = 0;
            string[] words = documentText.Split(' ');
            foreach (var word in words)
            {
                if (word.Length >= Configs.MIN_WORD_LENGTH)
                {
                    if (MainCache.MostCommonAlbanianWords.ContainsKey(word))
                        albWordCount++;
                    wordCount++;
                }
            }

            if (wordCount == 0)
                wordCount = 1;

            double albOtherRatio = (double)albWordCount / wordCount;
            return albOtherRatio;
        }

        /// <summary>
        /// Checks if given documents are same based on their content
        /// </summary>
        /// <param name="document1Content">content of document 1</param>
        /// <param name="document2Content">content of document 2</param>
        /// <returns></returns>
        public static bool haveSameContent(string document1Content, string document2Content)
        {
            if (document1Content.Equals(document2Content))
                return true;
            else
                return false;
        }
        #endregion
    }
}
