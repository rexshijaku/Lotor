using Lotor.Caches;
using Lotor.Globals;
using Lotor.Helpers;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Helpers
{
    public class GlobalHelper
    {
        /// <summary>
        /// reports the domain level which is currently being crawling 
        /// (invoked when level is changed)
        /// </summary>
        /// <param name="level">domain level - enum e.g level.first</param>
        public static void reportCrawlingLevel(Level level)
        {
            string message = "Currently crawling ";
            if (level == Level.First)
                message += "1st";
            else if (level == Level.Second)
                message += "2nd";
            else if (level == Level.Third)
                message += "3rd";
            message += " level.";
            if (level != Level.Index)
                Report.info(message, ConsoleColor.Magenta);
        }

        public static string levelStr(Level level)
        {
            if (level == Level.First)
                return "1st";
            else if (level == Level.Second)
                return "2nd";
            else if (level == Level.Third)
                return "3rd";
            else
                return "Index";
        }
        /// <summary>
        /// adds new links found in a document (of current level) to the next level documents list
        /// </summary>
        /// <param name="newLinks">links that are found in a document (links to be added)</param>
        /// <param name="listTo">links of level that will be crawled next</param>
        public static int addLinksToList(string currentlyDownloadingDocument, List<string> newLinks, List<string> listTo)
        {
            Report.info(currentlyDownloadingDocument + Report.separator + " Formating links for the next level...");

            int totalValid = 0;
            foreach (var link in newLinks)
            {
                LotorUrl url = new LotorUrl(link, currentlyDownloadingDocument);
                if (!url.UrlIsValid)
                {
                    Report.error(String.Format("{0} was not added! | Reason: {1}", url.Url, url.Message));
                    continue;
                }
                else
                    totalValid += UrlHelper.tryToAddUrl(url, listTo, link);
            }
            return totalValid;
        }

        /// <summary>
        /// checks whether the given document has the same content with a document which has a same weight
        /// first checks text, then html and so on
        /// </summary>
        /// <param name="currentDocument">document that is being processed</param>
        /// <param name="similarDocumentsUrl">document that was found with same weight</param>
        /// <returns></returns>
        public static bool sameContent(Document currentDocument, string similarDocumentsUrl)
        {
            Report.info("Comparing contents of " + currentDocument.url + " with " + similarDocumentsUrl, ConsoleColor.Yellow);
            string similarDocumentHtml = FileOperations.getTemporaryDocumentContent(similarDocumentsUrl);
            string similarDocumentText = TextOperations.getDocumentsText(similarDocumentHtml, similarDocumentsUrl, false);
            string currentDocumentsText = TextOperations.getDocumentsText(currentDocument.html, currentDocument.url, false);

            if (TextOperations.haveSameContent(currentDocumentsText, similarDocumentText))
            {
                if (TextOperations.imageCount(currentDocument.html) == TextOperations.imageCount(similarDocumentHtml))
                {
                    Report.error("Same source " + currentDocument.url + " with " + similarDocumentsUrl + "!");
                    return true;
                }
            }
            Report.success(similarDocumentsUrl + " is not the same source as " + currentDocument.url);
            return false;
        }

        /// <summary>
        /// saves uniuqe pages on file for a particular domain
        /// counts number of unique pages, and returns
        /// </summary>
        /// <param name="levelUrls"></param>
        /// <param name="isAlb"></param>
        /// <param name="level"></param>
        /// <returns>uniqueDocumentCount</returns>
        public static int saveLevelDocuments(List<string> levelUrls, bool isAlb, Level level)
        {
            int uniqueDocumentCount = 0;
            if (levelUrls.Count > 0)
            {
                string links = String.Empty;
                foreach (var link in levelUrls)
                {
                    if (!link.Equals(Constants.DUPLICATE_DOCUMENT_CONTENT) && !link.Equals(Constants.DOCUMENT_NOT_FOUND_CONTENT)) // exclude duplicates and not found documents
                    {
                        uniqueDocumentCount++;
                        links += link + Environment.NewLine;
                    }
                }
                FileOperations.saveUrl(links, "level_" + ((int)level) + "_" + (isAlb ? "ALB_" : "NONALB_"));
            }
            return uniqueDocumentCount;
        }

        /// <summary>
        /// calculates percentage of crawled documents
        /// </summary>
        /// <returns></returns>
        public static double getCrawledPercentage()
        {
            return (DomainCache.successfullyProcessed * 100) / DomainCache.totalUrls;
        }

        /// <summary>
        /// helper function
        /// </summary>
        public static void animateCrawlEnd()
        {
            for (int i = 0; i < 95; i++)
            {
                if (i == 47)
                    Console.Write("LOTOR");
                Console.Write("*");
                System.Threading.Thread.Sleep(15);
            }
            Console.WriteLine("");
        }

        /// <summary>
        /// helper function (returns time)
        /// </summary>
        public static string getTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
