using Lotor.Calculations;
using Lotor.Helpers;
using Lotor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotor.Caches
{
    /// <summary>
    /// This class holds the information for a domain that is being crawled 
    /// and its cache is disposed at the end of the each domain crawl
    /// </summary>
    public class DomainCache
    {
        /// <summary>
        /// stores domain name
        /// </summary>
        public static Domain activeDomain = null;

        /// <summary>
        /// stores documents and their corresponding weights in a key value pair 
        /// where key is documents url 
        /// and value is documents weight
        /// </summary>
        public static Dictionary<string, long> documentsAndWeights = new Dictionary<string, long>();

        public static int successfullyProcessed = 0;

        /// <summary>
        /// stores documents that are / or are likely to be Albanian
        /// </summary>
        public static Dictionary<string, double> likelyAlbanian = new Dictionary<string, double>();

        /// <summary>
        /// domain url, artificial level, used temporarily
        /// </summary>
        public static List<string> zeroLevel = new List<string>();


        /// <summary>
        /// stores first level document urls 
        /// in other words urls that are found in domains index page
        /// </summary>
        public static List<string> firstLevelUrls = new List<string>();

        /// <summary>
        /// stores second level documents
        /// </summary>
        public static List<string> secondLevelUrls = new List<string>();

        /// <summary>
        /// stores third level documents
        /// </summary>
        public static List<string> thirdLevelUrls = new List<string>();

        /// <summary>
        /// checks whether url is added before
        /// </summary>
        /// <param name="url">url which we check if it is added before</param>
        /// <returns>true if added, false if not</returns>
        public static bool isAddedBefore(string url)
        {
            return zeroLevel.Contains(url) || firstLevelUrls.Contains(url) || secondLevelUrls.Contains(url) || thirdLevelUrls.Contains(url);
        }

        /// <summary>
        /// returns total number of documents found in a domain
        /// called periodically
        /// </summary>
        public static int totalUrls
        {
            get
            {
                return (zeroLevel.Count + firstLevelUrls.Count + secondLevelUrls.Count + thirdLevelUrls.Count);
            }
        }

    }
}
